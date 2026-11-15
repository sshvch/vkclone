namespace user.DTOs;

public class DtoUpdateProfile
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Status { get; set; }
}
