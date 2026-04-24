using Microsoft.AspNetCore.Identity;

namespace FBL.Api.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public FantasyTeam? FantasyTeam { get; set; }
    public ICollection<LeagueMember> LeagueMemberships { get; set; } = new List<LeagueMember>();
}
