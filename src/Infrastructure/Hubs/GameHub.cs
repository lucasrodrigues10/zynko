using Microsoft.AspNetCore.SignalR;

namespace Zynko.Infrastructure.Hubs;

public class GameHub : Hub
{
    public async Task JoinGameGroup(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
    }

    public async Task LeaveGameGroup(string gameCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
    }
}
