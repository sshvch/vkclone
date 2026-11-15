using Content.Models;
using Content.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Content.Repositories;

public class CommentRepository
{
    private readonly IMongoCollection<Comment> _comments;

    public CommentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _comments = database.GetCollection<Comment>("comments");
    }

    public async Task InsertAsync(Comment comment) =>
        await _comments.InsertOneAsync(comment);

    public async Task<List<Comment>> GetByPostIdAsync(string postId) =>
        await _comments.Find(c => c.PostId == postId).SortBy(c => c.CreatedAt).ToListAsync();

    public async Task<Comment?> FindByIdAsync(string commentId) =>
        await _comments.Find(c => c.Id == commentId).FirstOrDefaultAsync();

    public async Task DeleteAsync(string commentId) =>
        await _comments.DeleteOneAsync(c => c.Id == commentId);

    public async Task DeleteByPostIdAsync(string postId) =>
        await _comments.DeleteManyAsync(c => c.PostId == postId);
}
