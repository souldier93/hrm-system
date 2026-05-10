using HrmSystem.API.Data;
using HrmSystem.API.Hubs;
using HrmSystem.API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HrmSystem.API.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext context,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Lấy danh sách thông báo của nhân viên
    public async Task<object> GetByEmployeeAsync(int employeeId, int page = 1)
    {
        var pageSize = 10;
        var total = await _context.Notifications
            .CountAsync(n => n.EmployeeId == employeeId);

        var items = await _context.Notifications
            .Where(n => n.EmployeeId == employeeId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync();

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.EmployeeId == employeeId && !n.IsRead);

        return new { items, total, unreadCount };
    }

    // Đánh dấu đã đọc
    public async Task MarkAsReadAsync(int id, int employeeId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id && n.EmployeeId == employeeId);
        if (notification == null) return;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    // Đánh dấu tất cả đã đọc
    public async Task MarkAllAsReadAsync(int employeeId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.EmployeeId == employeeId && !n.IsRead)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
    }

    // Gửi thông báo đến 1 nhân viên
    public async Task SendToEmployeeAsync(int employeeId,
        string title, string message, string type = "Info")
    {
        var notification = new Notification
        {
            EmployeeId = employeeId,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Push real-time qua SignalR
        await _hubContext.Clients
            .Group($"employee_{employeeId}")
            .SendAsync("NewNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.CreatedAt
            });
    }

    // Gửi thông báo đến tất cả nhân viên
    public async Task SendToAllAsync(string title,
        string message, string type = "Info")
    {
        var employees = await _context.Employees
            .Where(e => e.Status == "Active")
            .Select(e => e.Id)
            .ToListAsync();

        foreach (var employeeId in employees)
        {
            await SendToEmployeeAsync(employeeId, title, message, type);
        }
    }
}