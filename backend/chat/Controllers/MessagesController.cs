using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Chat.Services;

namespace Chat.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ChatService _chat;

    public MessagesController(ChatService chat)
    {
        _chat = chat;
    }

    // История переписки с пользователем
    [HttpGet("{peerId}")]
    public async Task<IActionResult> GetHistory(string peerId, [FromQuery] int limit = 100)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        limit = Math.Clamp(limit, 1, 200);

        var messages = await _chat.GetHistoryAsync(userId, peerId, limit);
        return Ok(messages);
    }
}
