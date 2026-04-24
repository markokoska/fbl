using Microsoft.AspNetCore.SignalR;

namespace FBL.Api.Hubs;

public class LiveScoreHub : Hub
{
    public async Task JoinGameweek(int gameweekId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"gw-{gameweekId}");
    }

    public async Task LeaveGameweek(int gameweekId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"gw-{gameweekId}");
    }
}
