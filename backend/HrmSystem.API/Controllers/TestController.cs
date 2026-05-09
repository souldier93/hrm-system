using HrmSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("hash")]
    public IActionResult GetHash([FromQuery] string password)
    {
        var hash = AuthService.HashPassword(password);
        return Ok(new { password, hash });
    }
}