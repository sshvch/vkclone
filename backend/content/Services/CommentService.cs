using Content.Models;
using Content.Repositories;

namespace Content.Services;

public class CommentService
{
    private const int MaxTextLength = 2000;

    private readonly CommentRepository _commentRepository;

    public CommentService(CommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Comment> AddCommentAsync(string postId, string authorId, string text)
    {
        if (string.IsNullOrWhiteSpace(postId))
            throw new InvalidOperationException("Не указан пост");

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Текст комментария пустой");

        if (text.Length > MaxTextLength)
            throw new InvalidOperationException($"Комментарий длиннее {MaxTextLength} символов");

        var comment = new Comment
        {
            PostId = postId,
            AuthorId = authorId,
            Text = text.Trim()
        };

        await _commentRepository.InsertAsync(comment);
        return comment;
    }

    public async Task<List<Comment>> GetCommentsAsync(string postId)
    {
        return await _commentRepository.GetByPostIdAsync(postId);
    }

    public Task<Comment?> FindByIdAsync(string commentId)
        => _commentRepository.FindByIdAsync(commentId);

    public Task DeleteAsync(string commentId)
        => _commentRepository.DeleteAsync(commentId);
}
