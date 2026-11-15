using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Chat.Models;
using Chat.Settings;

namespace Chat.Repositories;

public class MessageRepository
{
    private readonly IMongoCollection<ChatMessage> _messages;

    public MessageRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _messages = database.GetCollection<ChatMessage>("messages");
    }

    public Task AddAsync(ChatMessage message)
        => _messages.InsertOneAsync(message);

    public async Task<List<ChatMessage>> GetConversationAsync(string firstUserId, string secondUserId, int limit)
    {
        var messages = await _messages
            .Find(m =>
                (m.FromUserId == firstUserId && m.ToUserId == secondUserId) ||
                (m.FromUserId == secondUserId && m.ToUserId == firstUserId))
            .SortByDescending(m => m.CreatedAt)
            .Limit(limit)
            .ToListAsync();

        return messages.OrderBy(m => m.CreatedAt).ToList();
    }
}
