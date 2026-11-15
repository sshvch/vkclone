using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using user.Models;
using user.Settings;
namespace user.Repositories;

public class UserRepository
{

    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _users = database.GetCollection<User>("users");
    }
    public async Task<User?> FindUserById(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user is null) { return null; }
        return user;
    }

    public async Task CreateIfNotExistsAsync(User user)
    {
        var update = Builders<User>.Update
            .SetOnInsert(u => u.Email, user.Email)
            .SetOnInsert(u => u.FirstName, user.FirstName)
            .SetOnInsert(u => u.LastName, user.LastName)
            .SetOnInsert(u => u.CreatedAt, user.CreatedAt);

        await _users.UpdateOneAsync(
            u => u.Id == user.Id,
            update,
            new UpdateOptions { IsUpsert = true });
    }

    public async Task UpdateAsync(User user) =>
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

    public async Task<List<User>> GetUsersAsync(
        string excludeUserId,
        int page,
        int pageSize,
        string? search)
    {
        return await _users
            .Find(BuildFilter(excludeUserId, search))
            .SortBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public Task<long> CountUsersAsync(string excludeUserId, string? search)
        => _users.CountDocumentsAsync(BuildFilter(excludeUserId, search));

    public async Task<List<User>> FindByIdsAsync(IEnumerable<string> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new List<User>();

        return await _users
            .Find(Builders<User>.Filter.In(u => u.Id, idList))
            .ToListAsync();
    }

    private static FilterDefinition<User> BuildFilter(string excludeUserId, string? search)
    {
        var builder = Builders<User>.Filter;
        var filter = builder.Ne(u => u.Id, excludeUserId) & builder.Ne(u => u.IsDeleted, true);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = new BsonRegularExpression(Regex.Escape(search.Trim()), "i");
            filter &= builder.Or(
                builder.Regex(u => u.FirstName, pattern),
                builder.Regex(u => u.LastName, pattern));
        }

        return filter;
    }
}
