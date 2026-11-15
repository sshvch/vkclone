using System.Security.Claims;
using Content.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Controllers;

[ApiController]
[Authorize]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly PostService _postService;

    public PostController(PostService postService)
    {
        _postService = postService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Создать пост
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var authorId = CurrentUserId;
        if (string.IsNullOrEmpty(authorId))
            return Unauthorized(new { message = "Id = Null" });

        try
        {
            var post = await _postService.CreatePostAsync(authorId, request.Text, request.ImageUrl);
            return Ok(post);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Лента постов
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _postService.GetFeedAsync(page, pageSize);
        return Ok(new { items, total, page, pageSize });
    }

    // Удалить пост
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var post = await _postService.FindByIdAsync(id);
        if (post is null)
            return NotFound(new { message = "Пост не найден" });

        if (post.AuthorId != userId)
            return StatusCode(403, new { message = "Это не ваш пост" });

        await _postService.DeleteAsync(id);
        return NoContent();
    }

    // Поставить лайк
    [HttpPost("{id}/like")]
    public async Task<IActionResult> Like(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var post = await _postService.LikeAsync(id, userId);
        if (post is null)
            return NotFound(new { message = "Пост не найден" });

        return Ok(post);
    }

    // Убрать лайк
    [HttpDelete("{id}/like")]
    public async Task<IActionResult> Unlike(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var post = await _postService.UnlikeAsync(id, userId);
        if (post is null)
            return NotFound(new { message = "Пост не найден" });

        return Ok(post);
    }
}

public class CreatePostRequest
{
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
