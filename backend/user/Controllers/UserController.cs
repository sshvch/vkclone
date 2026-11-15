using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user.DTOs;
using user.Services;

namespace user.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private Task<DtoUser?> EnsureProfileAsync(string userId) =>
        _userService.GetOrCreateUserAsync(
            userId,
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Surname) ?? string.Empty);

    // Мой профиль
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var user = await EnsureProfileAsync(userId);

        if (user is null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    // Редактировать профиль
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] DtoUpdateProfile request)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        await EnsureProfileAsync(userId);

        try
        {
            var user = await _userService.UpdateProfileAsync(userId, request);
            if (user is null)
                return NotFound(new { message = "Пользователь не найден" });

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Загрузить аватар
    [HttpPost("profile/avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        await EnsureProfileAsync(userId);

        try
        {
            var user = await _userService.UpdateAvatarAsync(userId, file);
            if (user is null)
                return NotFound(new { message = "Пользователь не найден" });

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Удалить аватар
    [HttpDelete("profile/avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        await EnsureProfileAsync(userId);

        var user = await _userService.DeleteAvatarAsync(userId);
        if (user is null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    // Список пользователей: поиск и пагинация
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _userService.GetUsersAsync(userId, page, pageSize, search);

        return Ok(new { items, total, page, pageSize });
    }

    // Мои друзья
    [HttpGet("me/friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var friends = await _userService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    // Профиль другого пользователя
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        var user = await _userService.GetPublicUserAsync(userId, id);
        if (user is null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    // Удалить из друзей
    [HttpDelete("me/friends/{id}")]
    public async Task<IActionResult> RemoveFriend(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Id = Null" });

        await _userService.RemoveFriendAsync(userId, id);
        return NoContent();
    }
}
