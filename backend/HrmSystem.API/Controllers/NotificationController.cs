using HrmSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Lấy thông báo của mình
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page = 1)
    {
        var employeeId = int.Parse(
            User.FindFirstValue("EmployeeId")!);
        var result = await _notificationService
            .GetByEmployeeAsync(employeeId, page);
        return Ok(result);
    }

    // Đánh dấu đã đọc 1 thông báo
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var employeeId = int.Parse(
            User.FindFirstValue("EmployeeId")!);
        await _notificationService.MarkAsReadAsync(id, employeeId);
        return Ok();
    }

    // Đánh dấu tất cả đã đọc
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var employeeId = int.Parse(
            User.FindFirstValue("EmployeeId")!);
        await _notificationService.MarkAllAsReadAsync(employeeId);
        return Ok();
    }

    // Admin gửi thông báo đến tất cả
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Broadcast(
        [FromBody] BroadcastRequest req)
    {
        await _notificationService.SendToAllAsync(
            req.Title, req.Message, req.Type);
        return Ok(new { message = "Đã gửi thông báo!" });
    }
}

public class BroadcastRequest
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "Info";
}