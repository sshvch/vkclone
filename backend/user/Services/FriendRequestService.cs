using user.DTOs;
using user.Models;
using user.Repositories;

namespace user.Services;

public class FriendRequestService
{
    private readonly FriendshipRepository _friendships;
    private readonly UserRepository _users;

    public FriendRequestService(
        FriendshipRepository friendships,
        UserRepository users)
    {
        _friendships = friendships;
        _users = users;
    }

    public async Task<DtoFriendRequest> SendAsync(string fromUserId, string toUserId)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("Нельзя отправить заявку самому себе");

        var me = await _users.FindUserById(fromUserId)
            ?? throw new InvalidOperationException("Ваш профиль не найден");

        if (await _users.FindUserById(toUserId) is null)
            throw new InvalidOperationException("Пользователь не найден");

        var existing = await _friendships.FindBetweenAsync(fromUserId, toUserId);

        if (existing?.Status == FriendshipStatus.Accepted)
            throw new InvalidOperationException("Пользователь уже у вас в друзьях");

        if (existing?.Status == FriendshipStatus.Pending)
            throw new InvalidOperationException("Заявка уже отправлена или ожидает ответа");

        Friendship request;

        if (existing is not null)
        {
            existing.Status = FriendshipStatus.Pending;
            existing.RequestedBy = fromUserId;
            existing.IsRead = false;
            existing.CreatedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _friendships.UpdateAsync(existing);
            request = existing;
        }
        else
        {
            var (userA, userB) = FriendshipRepository.OrderPair(fromUserId, toUserId);

            request = new Friendship
            {
                UserA = userA,
                UserB = userB,
                Status = FriendshipStatus.Pending,
                RequestedBy = fromUserId
            };

            await _friendships.CreateAsync(request);
        }

        var friendsCount = (int)await _friendships.CountAcceptedAsync(fromUserId);
        return new DtoFriendRequest(request, me, friendsCount);
    }

    public async Task<List<DtoFriendRequest>> GetIncomingAsync(string userId)
    {
        var incoming = await _friendships.GetIncomingPendingAsync(userId);

        var senders = await _users.FindByIdsAsync(incoming.Select(f => f.RequestedBy));
        var byId = senders.ToDictionary(u => u.Id);
        var counts = await _friendships.CountAcceptedForUsersAsync(senders.Select(u => u.Id));

        return incoming
            .Select(f => new DtoFriendRequest(
                f,
                byId.GetValueOrDefault(f.RequestedBy),
                counts.GetValueOrDefault(f.RequestedBy)))
            .ToList();
    }

    public async Task AcceptAsync(string userId, string requestId)
    {
        var request = await GetOwnPendingAsync(userId, requestId);

        request.Status = FriendshipStatus.Accepted;
        request.IsRead = true;
        request.UpdatedAt = DateTime.UtcNow;

        await _friendships.UpdateAsync(request);
    }

    public async Task DeclineAsync(string userId, string requestId)
    {
        var request = await GetOwnPendingAsync(userId, requestId);

        request.Status = FriendshipStatus.Declined;
        request.IsRead = true;
        request.UpdatedAt = DateTime.UtcNow;

        await _friendships.UpdateAsync(request);
    }

    public async Task MarkReadAsync(string userId, string requestId)
    {
        var request = await GetOwnPendingAsync(userId, requestId);
        await _friendships.MarkReadAsync(request.Id);
    }

    private async Task<Friendship> GetOwnPendingAsync(string userId, string requestId)
    {
        var request = await _friendships.FindByIdAsync(requestId)
            ?? throw new InvalidOperationException("Заявка не найдена");

        if (request.UserA != userId && request.UserB != userId)
            throw new InvalidOperationException("Это не ваша заявка");

        if (request.RequestedBy == userId)
            throw new InvalidOperationException("Нельзя принять или отклонить свою заявку");

        if (request.Status != FriendshipStatus.Pending)
            throw new InvalidOperationException("Заявка уже обработана");

        return request;
    }
}
