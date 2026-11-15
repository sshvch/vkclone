using user.Models;

namespace user.DTOs;

public class DtoUser
{
    public DtoUser() { }

    public DtoUser(User user, int friendsCount)
    {
        Id = user.Id;
        Email = user.Email;
        FirstName = user.FirstName;
        LastName = user.LastName;
        City = user.City;
        Phone = user.Phone;
        AvatarUrl = user.AvatarUrl;
        Status = user.Status;
        FriendsCount = friendsCount;
        CreatedAt = user.CreatedAt;
    }

    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Status { get; set; }
    public int FriendsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
