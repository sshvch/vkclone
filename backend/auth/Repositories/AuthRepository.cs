using auth.DTOs.Auth;
using auth.Models;
using Auth.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace auth.Repositories;

public class AuthRepository
{
    private readonly IMongoCollection<User> _users;

    public AuthRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _users = database.GetCollection<User>("users");
    }

    public async Task<DtoUserResponse?> FindUserById(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user is null ? null : new DtoUserResponse(user);
    }

    public async Task<DtoUserResponse?> CreateUserAsync(DtoCreateUser newUser)
    {
        var isExists = await _users.Find(u => u.Email == newUser.email).AnyAsync();
        if (isExists)
            throw new Exception("Пользователь с таким email уже существует!");

        User user = new User
        {
            FirstName = newUser.firstName,
            LastName = newUser.lastName,
            PasswordHash = newUser.password,
            Email = newUser.email,
        };

        await _users.InsertOneAsync(user);
        return new DtoUserResponse(user);
    }

    public async Task<DtoUserResponse> CheckUserAsync(string email, string password)
    {
        var user = await _users
            .Find(u => u.PasswordHash == password && u.Email == email)
            .FirstOrDefaultAsync();

        if (user is null)
            throw new Exception("Такого пользователя не существует");

        return new DtoUserResponse(user);
    }
}
