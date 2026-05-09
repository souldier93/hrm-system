using HrmSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // Lấy chấm công theo tháng (Admin/Manager)
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetByMonth(
        [FromQuery] int month, [FromQuery] int year)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;
        var result = await _attendanceService.GetByMonthAsync(month, year);
        return Ok(result);
    }

    // Lấy chấm công hôm nay của chính mình
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var employeeId = int.Parse(
            User.FindFirstValue("EmployeeId")!);
        var result = await _attendanceService.GetTodayAsync(employeeId);
        return Ok(result);
    }

    // Check in
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn()
    {
        var employeeId = int.Parse(
            User.FindFirstValue("EmployeeId")!);
        try
        {
            var result = await _attendanceService.CheckInAsync(employeeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Check out
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut()
    {
        var employeeId = int.Parse(
            User.FindFirstValue("EmployeeId")!);
        try
        {
            var result = await _attendanceService.CheckOutAsync(employeeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Admin chỉnh sửa chấm công
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceRequest req)
    {
        var success = await _attendanceService.UpdateAsync(
            id, req.Date, req.CheckIn, req.CheckOut, req.Status, req.Notes);
        if (!success) return NotFound();
        return Ok();
    }
}

public class UpdateAttendanceRequest
{
    public DateOnly Date { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}