using Microsoft.AspNetCore.SignalR;

namespace Zynko.Web.Hubs;

public class GameHub : Hub
{
    public Task Ping() => Clients.Caller.SendAsync("Pong");
}
