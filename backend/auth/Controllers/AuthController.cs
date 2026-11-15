using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using auth.DTOs.Auth;
using auth.Services;

namespace auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthControllers : ControllerBase
{
    private readonly AuthService _authService;

    public AuthControllers(AuthService authService)
    {
        _authService = authService;
    }

    // Текущий пользователь
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userId)) {return Unauthorized(new {message = "userId = Null"});}

        var user = await _authService.GetUserById(userId);

        if (user is null ){return BadRequest(new {message = "Пользователь не найден"});}
        return Ok(user);
    }

    // Регистрация
    [HttpPost("regist")]
    public async Task<IActionResult> CreateUser([FromBody] DtoCreateUser request)
    {
        try
        {
            var result = await _authService.CreateUserAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

    }

    // Вход
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DtoLoginUser request)
    {
        try
        {
            var token = await _authService.CreateToken(request.email, request.password);
            return Ok(new { token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
