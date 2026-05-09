using HrmSystem.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionController : ControllerBase
{
    private readonly AppDbContext _context;
    public PositionController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _context.Positions
            .Select(p => new { p.Id, p.Name, p.BaseSalary })
            .ToListAsync();
        return Ok(result);
    }
}