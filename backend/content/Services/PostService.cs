using Content.Models;
using Content.Repositories;

namespace Content.Services;

public class PostService
{
    private const int MaxTextLength = 5000;

    private readonly PostRepository _postRepository;
    private readonly CommentRepository _commentRepository;

    public PostService(PostRepository postRepository, CommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task<Post> CreatePostAsync(string authorId, string text, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Текст поста пустой");

        if (text.Length > MaxTextLength)
            throw new InvalidOperationException($"Пост длиннее {MaxTextLength} символов");

        var post = new Post
        {
            AuthorId = authorId,
            Text = text.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim()
        };

        await _postRepository.InsertAsync(post);
        return post;
    }

    public async Task<(List<Post> Items, long Total)> GetFeedAsync(int page, int pageSize)
    {
        var posts = await _postRepository.GetFeedAsync(page, pageSize);
        var total = await _postRepository.CountAsync();
        return (posts, total);
    }

    public Task<Post?> FindByIdAsync(string postId)
        => _postRepository.FindByIdAsync(postId);

    public async Task DeleteAsync(string postId)
    {
        await _postRepository.DeleteAsync(postId);
        await _commentRepository.DeleteByPostIdAsync(postId);
    }

    public Task<Post?> LikeAsync(string postId, string userId)
        => _postRepository.LikeAsync(postId, userId);

    public Task<Post?> UnlikeAsync(string postId, string userId)
        => _postRepository.UnlikeAsync(postId, userId);
}
