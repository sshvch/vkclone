using auth.Models;

namespace auth.DTOs.Auth;

public class DtoUserResponse
{
    public DtoUserResponse() { }

    public DtoUserResponse(User user)
    {
        Id = user.Id;
        firstName = user.FirstName;
        lastName = user.LastName;
        email = user.Email;
        createdAt = user.CreatedAt;
    }

    public string Id { get; set; } = string.Empty;
    public string firstName { get; set; } = string.Empty;
    public string lastName { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public DateTime createdAt { get; set; }
}
