using Content.Models;
using Content.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Content.Repositories;

public class PostRepository
{
    private readonly IMongoCollection<Post> _posts;

    public PostRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _posts = database.GetCollection<Post>("posts");
    }

    public async Task InsertAsync(Post post) =>
        await _posts.InsertOneAsync(post);

    public async Task<List<Post>> GetFeedAsync(int page, int pageSize) =>
        await _posts.Find(_ => true)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

    public Task<long> CountAsync() =>
        _posts.CountDocumentsAsync(_ => true);

    public async Task<Post?> FindByIdAsync(string postId) =>
        await _posts.Find(p => p.Id == postId).FirstOrDefaultAsync();

    public async Task DeleteAsync(string postId) =>
        await _posts.DeleteOneAsync(p => p.Id == postId);

    public async Task<Post?> LikeAsync(string postId, string userId)
    {
        var post = await FindByIdAsync(postId);
        if (post is null) return null;

        if (post.Likes.Contains(userId)) return post;

        post.Likes.Add(userId);
        post.LikesCount = post.Likes.Count;

        await _posts.ReplaceOneAsync(p => p.Id == postId, post);
        return post;
    }

    public async Task<Post?> UnlikeAsync(string postId, string userId)
    {
        var post = await FindByIdAsync(postId);
        if (post is null) return null;

        if (!post.Likes.Contains(userId)) return post;

        post.Likes.Remove(userId);
        post.LikesCount = post.Likes.Count;

        await _posts.ReplaceOneAsync(p => p.Id == postId, post);
        return post;
    }
}
