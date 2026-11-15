using user.Models;

namespace user.DTOs;

public class DtoFriendRequest
{
    public DtoFriendRequest() { }

    public DtoFriendRequest(Friendship friendship, User? fromUser, int fromUserFriendsCount)
    {
        Id = friendship.Id;
        FromUserId = friendship.RequestedBy;
        ToUserId = friendship.UserA == friendship.RequestedBy
            ? friendship.UserB
            : friendship.UserA;
        Status = friendship.Status;
        IsRead = friendship.IsRead;
        CreatedAt = friendship.CreatedAt;
        FromUser = fromUser is null
            ? null
            : new DtoUserListItem(fromUser, fromUserFriendsCount, false, false);
    }

    public string Id { get; set; } = string.Empty;
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public DtoUserListItem? FromUser { get; set; }
    public string Status { get; set; } = FriendshipStatus.Pending;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
