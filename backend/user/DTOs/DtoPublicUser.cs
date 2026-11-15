using user.Models;

namespace user.DTOs;

public class DtoPublicUser
{
    public DtoPublicUser() { }

    public DtoPublicUser(User user, int friendsCount, bool isFriend)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        AvatarUrl = user.AvatarUrl;
        City = user.City;
        Status = user.Status;
        FriendsCount = friendsCount;
        CreatedAt = user.CreatedAt;
        IsFriend = isFriend;
    }

    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public string? Status { get; set; }
    public int FriendsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFriend { get; set; }
}
