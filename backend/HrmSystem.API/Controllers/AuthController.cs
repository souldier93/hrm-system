using HrmSystem.API.DTOs;
using HrmSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });

        return Ok(result);
    }

    [HttpGet("hash")]
    public IActionResult GetHash([FromQuery] string password)
    {
        var hash = AuthService.HashPassword(password);
        return Ok(new { password, hash });
    }
}