using System.Security.Claims;
using Content.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Controllers;

[ApiController]
[Authorize]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService)
    {
        _commentService = commentService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Добавить комментарий
    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
    {
        var authorId = CurrentUserId;
        if (string.IsNullOrEmpty(authorId))
            return Unauthorized(new { message = "Id = Null" });

        try
        {
            var comment = await _commentService.AddCommentAsync(request.PostId, authorId, request.Text);
            return Ok(comment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Комментарии к посту
    [HttpGet("{postId}")]
    public async Task<IActionResult> GetComments(string postId)
    {
        var comments = await _commentService.GetCommentsAsync(postId);
        return Ok(comments);
    }

    // Удалить комментарий
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var comment = await _commentService.FindByIdAsync(id);
        if (comment is null)
            return NotFound(new { message = "Комментарий не найден" });

        if (comment.AuthorId != userId)
            return StatusCode(403, new { message = "Это не ваш комментарий" });

        await _commentService.DeleteAsync(id);
        return NoContent();
    }
}

public class AddCommentRequest
{
    public string PostId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
