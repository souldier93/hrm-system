using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HrmSystem.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirstValue(ClaimTypes.Role);
        var employeeId = Context.User?.FindFirstValue("EmployeeId");

        if (role != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, role);

        // Thêm vào group cá nhân theo employeeId
        if (employeeId != null)
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"employee_{employeeId}");

        await base.OnConnectedAsync();
    }
}