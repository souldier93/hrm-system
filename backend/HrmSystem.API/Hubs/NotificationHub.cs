using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HrmSystem.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(
            System.Security.Claims.ClaimTypes.Role)?.Value;

        // Gom nhân viên vào group theo role
        if (role != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, role);

        await base.OnConnectedAsync();
    }
}