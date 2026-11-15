using user.DTOs;
using user.Models;
using user.Repositories;

namespace user.Services;

public class UserService
{
    private const int MaxNameLength = 50;
    private const int MaxStatusLength = 200;
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private static readonly string[] AvatarExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly UserRepository _userRepository;
    private readonly FriendshipRepository _friendships;
    private readonly IWebHostEnvironment _env;

    public UserService(
        UserRepository userRepository,
        FriendshipRepository friendships,
        IWebHostEnvironment env)
    {
        _userRepository = userRepository;
        _friendships = friendships;
        _env = env;
    }

    public async Task<DtoUser?> GetOrCreateUserAsync(
        string userId,
        string email,
        string firstName,
        string lastName)
    {
        var user = await _userRepository.FindUserById(userId);

        if (user is null)
        {
            user = new User
            {
                Id = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateIfNotExistsAsync(user);
            user = await _userRepository.FindUserById(userId);
        }

        if (user is null) { return null; }

        var friendsCount = (int)await _friendships.CountAcceptedAsync(userId);
        return new DtoUser(user, friendsCount);
    }

    public async Task<(List<DtoUserListItem> Items, long Total)> GetUsersAsync(
        string currentUserId,
        int page,
        int pageSize,
        string? search)
    {
        var users = await _userRepository.GetUsersAsync(currentUserId, page, pageSize, search);
        var total = await _userRepository.CountUsersAsync(currentUserId, search);

        var friendIds = (await _friendships.GetFriendIdsAsync(currentUserId)).ToHashSet();
        var outgoingIds = (await _friendships.GetOutgoingPendingIdsAsync(currentUserId))
            .ToHashSet();
        var counts = await _friendships.CountAcceptedForUsersAsync(users.Select(u => u.Id));

        var items = users
            .Select(u => new DtoUserListItem(
                u,
                counts.GetValueOrDefault(u.Id),
                friendIds.Contains(u.Id),
                outgoingIds.Contains(u.Id)))
            .ToList();

        return (items, total);
    }

    public async Task<List<DtoUserListItem>> GetFriendsAsync(string userId)
    {
        var friendIds = await _friendships.GetFriendIdsAsync(userId);
        var friends = await _userRepository.FindByIdsAsync(friendIds);
        var counts = await _friendships.CountAcceptedForUsersAsync(friends.Select(f => f.Id));

        return friends
            .OrderBy(f => f.FirstName)
            .ThenBy(f => f.LastName)
            .Select(f => new DtoUserListItem(
                f,
                counts.GetValueOrDefault(f.Id),
                isFriend: true,
                hasOutgoingRequest: false))
            .ToList();
    }

    public async Task<DtoPublicUser?> GetPublicUserAsync(string currentUserId, string targetUserId)
    {
        var target = await _userRepository.FindUserById(targetUserId);
        if (target is null) return null;

        var friendship = await _friendships.FindBetweenAsync(currentUserId, targetUserId);
        var isFriend = friendship?.Status == FriendshipStatus.Accepted;
        var friendsCount = (int)await _friendships.CountAcceptedAsync(targetUserId);

        return new DtoPublicUser(target, friendsCount, isFriend);
    }

    public Task RemoveFriendAsync(string userId, string friendId)
        => _friendships.DeleteBetweenAsync(userId, friendId);

    public async Task<DtoUser?> UpdateProfileAsync(string userId, DtoUpdateProfile request)
    {
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            throw new InvalidOperationException("Имя и фамилия обязательны");

        if (firstName.Length > MaxNameLength || lastName.Length > MaxNameLength)
            throw new InvalidOperationException($"Имя и фамилия не длиннее {MaxNameLength} символов");

        if (request.Status?.Trim().Length > MaxStatusLength)
            throw new InvalidOperationException($"Статус не длиннее {MaxStatusLength} символов");

        var user = await _userRepository.FindUserById(userId);
        if (user is null) return null;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.City = Clean(request.City);
        user.Phone = Clean(request.Phone);
        user.Status = Clean(request.Status);

        await _userRepository.UpdateAsync(user);

        var friendsCount = (int)await _friendships.CountAcceptedAsync(userId);
        return new DtoUser(user, friendsCount);
    }

    public async Task<DtoUser?> UpdateAvatarAsync(string userId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("Файл не выбран");

        if (file.Length > MaxAvatarBytes)
            throw new InvalidOperationException("Аватар больше 2 МБ");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AvatarExtensions.Contains(extension))
            throw new InvalidOperationException("Можно загрузить jpg, png, webp или gif");

        var user = await _userRepository.FindUserById(userId);
        if (user is null) return null;

        var folder = AvatarFolder();
        Directory.CreateDirectory(folder);
        DeleteAvatarFiles(folder, userId);

        var fileName = userId + extension;

        using (var stream = File.Create(Path.Combine(folder, fileName)))
        {
            await file.CopyToAsync(stream);
        }

        user.AvatarUrl = "/avatars/" + fileName;
        await _userRepository.UpdateAsync(user);

        var friendsCount = (int)await _friendships.CountAcceptedAsync(userId);
        return new DtoUser(user, friendsCount);
    }

    public async Task<DtoUser?> DeleteAvatarAsync(string userId)
    {
        var user = await _userRepository.FindUserById(userId);
        if (user is null) return null;

        var folder = AvatarFolder();
        if (Directory.Exists(folder)) DeleteAvatarFiles(folder, userId);

        user.AvatarUrl = null;
        await _userRepository.UpdateAsync(user);

        var friendsCount = (int)await _friendships.CountAcceptedAsync(userId);
        return new DtoUser(user, friendsCount);
    }

    private string AvatarFolder()
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, "avatars");
    }

    private static void DeleteAvatarFiles(string folder, string userId)
    {
        foreach (var path in Directory.GetFiles(folder, userId + ".*"))
            File.Delete(path);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
