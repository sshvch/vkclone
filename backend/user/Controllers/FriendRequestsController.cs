using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user.Services;

namespace user.Controllers;

[ApiController]
[Authorize]
[Route("api/users/me/friend-requests")]
public class FriendRequestsController : ControllerBase
{
    private readonly FriendRequestService _friendRequests;

    public FriendRequestsController(FriendRequestService friendRequests)
    {
        _friendRequests = friendRequests;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Отправить заявку
    [HttpPost("{userId}")]
    public async Task<IActionResult> Send(string userId)
    {
        var currentUserId = CurrentUserId;
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new { message = "Id = Null" });

        try
        {
            var request = await _friendRequests.SendAsync(currentUserId, userId);
            return Ok(request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Входящие заявки
    [HttpGet]
    public async Task<IActionResult> GetIncoming()
    {
        var currentUserId = CurrentUserId;
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new { message = "Id = Null" });

        var requests = await _friendRequests.GetIncomingAsync(currentUserId);
        return Ok(requests);
    }

    // Принять заявку
    [HttpPost("{id}/accept")]
    public Task<IActionResult> Accept(string id) => Process(id, _friendRequests.AcceptAsync);

    // Отклонить заявку
    [HttpPost("{id}/decline")]
    public Task<IActionResult> Decline(string id) => Process(id, _friendRequests.DeclineAsync);

    // Отметить заявку прочитанной
    [HttpPost("{id}/read")]
    public Task<IActionResult> MarkRead(string id) => Process(id, _friendRequests.MarkReadAsync);

    private async Task<IActionResult> Process(
        string id,
        Func<string, string, Task> action)
    {
        var currentUserId = CurrentUserId;
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new { message = "Id = Null" });

        try
        {
            await action(currentUserId, id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
