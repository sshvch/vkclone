using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user.Models;
using user.Settings;

namespace user.Repositories;

public class FriendshipRepository
{
    private readonly IMongoCollection<Friendship> _friendships;

    public FriendshipRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _friendships = database.GetCollection<Friendship>("friendships");
    }

    public static void EnsureIndexes(IMongoDatabase database)
    {
        var friendships = database.GetCollection<Friendship>("friendships");

        friendships.Indexes.CreateOne(new CreateIndexModel<Friendship>(
            Builders<Friendship>.IndexKeys.Ascending(f => f.UserA).Ascending(f => f.UserB),
            new CreateIndexOptions { Unique = true, Name = "ux_friendship_pair" }));
    }

    public static (string UserA, string UserB) OrderPair(string firstUserId, string secondUserId)
        => string.CompareOrdinal(firstUserId, secondUserId) <= 0
            ? (firstUserId, secondUserId)
            : (secondUserId, firstUserId);

    public async Task<Friendship?> FindByIdAsync(string id)
        => await _friendships.Find(f => f.Id == id).FirstOrDefaultAsync();

    public async Task<Friendship?> FindBetweenAsync(string firstUserId, string secondUserId)
    {
        var (userA, userB) = OrderPair(firstUserId, secondUserId);
        return await _friendships.Find(f => f.UserA == userA && f.UserB == userB)
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> GetFriendIdsAsync(string userId)
    {
        var friendships = await _friendships
            .Find(f => f.Status == FriendshipStatus.Accepted &&
                       (f.UserA == userId || f.UserB == userId))
            .ToListAsync();

        return friendships
            .Select(f => f.UserA == userId ? f.UserB : f.UserA)
            .ToList();
    }

    public async Task<List<Friendship>> GetIncomingPendingAsync(string userId)
        => await _friendships
            .Find(f => f.Status == FriendshipStatus.Pending &&
                       f.RequestedBy != userId &&
                       (f.UserA == userId || f.UserB == userId))
            .SortByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<List<string>> GetOutgoingPendingIdsAsync(string userId)
    {
        var friendships = await _friendships
            .Find(f => f.Status == FriendshipStatus.Pending && f.RequestedBy == userId)
            .ToListAsync();

        return friendships
            .Select(f => f.UserA == userId ? f.UserB : f.UserA)
            .ToList();
    }

    public Task CreateAsync(Friendship friendship)
        => _friendships.InsertOneAsync(friendship);

    public Task UpdateAsync(Friendship friendship)
        => _friendships.ReplaceOneAsync(f => f.Id == friendship.Id, friendship);

    public async Task DeleteBetweenAsync(string firstUserId, string secondUserId)
    {
        var (userA, userB) = OrderPair(firstUserId, secondUserId);
        await _friendships.DeleteOneAsync(f => f.UserA == userA && f.UserB == userB);
    }

    public Task MarkReadAsync(string friendshipId)
        => _friendships.UpdateOneAsync(
            f => f.Id == friendshipId,
            Builders<Friendship>.Update
                .Set(f => f.IsRead, true)
                .Set(f => f.UpdatedAt, DateTime.UtcNow));

    public Task<long> CountAcceptedAsync(string userId)
        => _friendships.CountDocumentsAsync(
            f => f.Status == FriendshipStatus.Accepted &&
                 (f.UserA == userId || f.UserB == userId));

    public async Task<Dictionary<string, int>> CountAcceptedForUsersAsync(IEnumerable<string> userIds)
    {
        var ids = userIds.Distinct().ToList();
        var counts = ids.ToDictionary(id => id, _ => 0);
        if (ids.Count == 0) return counts;

        var asUserA = await _friendships.Aggregate()
            .Match(f => f.Status == FriendshipStatus.Accepted && ids.Contains(f.UserA))
            .Group(f => f.UserA, group => new { Id = group.Key, Count = group.Count() })
            .ToListAsync();

        var asUserB = await _friendships.Aggregate()
            .Match(f => f.Status == FriendshipStatus.Accepted && ids.Contains(f.UserB))
            .Group(f => f.UserB, group => new { Id = group.Key, Count = group.Count() })
            .ToListAsync();

        foreach (var row in asUserA.Concat(asUserB))
        {
            if (counts.ContainsKey(row.Id))
                counts[row.Id] += row.Count;
        }

        return counts;
    }
}
