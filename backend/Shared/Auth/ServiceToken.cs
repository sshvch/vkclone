using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Auth;

public class ServiceToken
{
    private readonly IConfiguration _config;

    public ServiceToken(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string email, string userId, string firstName, string lastName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Key"]!));
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
