using user.Models;

namespace user.DTOs;

public class DtoUserListItem
{
    public DtoUserListItem() { }

    public DtoUserListItem(User user, int friendsCount, bool isFriend, bool hasOutgoingRequest)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        AvatarUrl = user.AvatarUrl;
        City = user.City;
        FriendsCount = friendsCount;
        IsFriend = isFriend;
        HasOutgoingRequest = hasOutgoingRequest;
    }

    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public int FriendsCount { get; set; }
    public bool IsFriend { get; set; }
    public bool HasOutgoingRequest { get; set; }
}
