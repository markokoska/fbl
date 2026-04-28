using Microsoft.AspNetCore.SignalR;

namespace FBL.Api.Hubs;

/// <summary>
/// SignalR hub for the live draft room. Clients join a league-scoped group and
/// receive PickMade / DraftStarted / DraftCompleted broadcasts pushed by DraftService.
/// </summary>
public class DraftHub : Hub
{
    public Task JoinDraft(int leagueId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"draft-{leagueId}");

    public Task LeaveDraft(int leagueId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"draft-{leagueId}");
}
