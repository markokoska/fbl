using Microsoft.AspNetCore.Identity;

namespace FBL.Api.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// A user can have multiple teams: optionally one Global team, plus one team per
    /// Classic/Draft league they're in.
    /// </summary>
    public ICollection<FantasyTeam> FantasyTeams { get; set; } = new List<FantasyTeam>();

    public ICollection<LeagueMember> LeagueMemberships { get; set; } = new List<LeagueMember>();
}
