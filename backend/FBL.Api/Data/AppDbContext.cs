using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FBL.Api.Models;

namespace FBL.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BundesligaPlayer> BundesligaPlayers => Set<BundesligaPlayer>();
    public DbSet<FantasyTeam> FantasyTeams => Set<FantasyTeam>();
    public DbSet<FantasyPick> FantasyPicks => Set<FantasyPick>();
    public DbSet<Gameweek> Gameweeks => Set<Gameweek>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueMember> LeagueMembers => Set<LeagueMember>();
    public DbSet<ChipUsage> ChipUsages => Set<ChipUsage>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<DraftPick> DraftPicks => Set<DraftPick>();
    public DbSet<WaiverClaim> WaiverClaims => Set<WaiverClaim>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- User → FantasyTeams (1:N, was 1:1) ----
        builder.Entity<AppUser>()
            .HasMany(u => u.FantasyTeams)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId);

        // ---- FantasyTeam → League (N:1, optional — null means Global team) ----
        builder.Entity<FantasyTeam>()
            .HasOne(t => t.League)
            .WithMany(l => l.Teams)
            .HasForeignKey(t => t.LeagueId)
            .IsRequired(false);

        // A user can have at most one team per league. (For LeagueId = NULL, Postgres
        // treats NULLs as distinct so this won't enforce one-global-per-user — that's
        // enforced in TeamController instead.)
        builder.Entity<FantasyTeam>()
            .HasIndex(t => new { t.UserId, t.LeagueId })
            .IsUnique();

        builder.Entity<FantasyPick>()
            .HasOne(p => p.FantasyTeam)
            .WithMany(t => t.Picks)
            .HasForeignKey(p => p.FantasyTeamId);

        builder.Entity<FantasyPick>()
            .HasOne(p => p.Player)
            .WithMany(pl => pl.FantasyPicks)
            .HasForeignKey(p => p.PlayerId);

        builder.Entity<MatchEvent>()
            .HasOne(e => e.Player)
            .WithMany(p => p.MatchEvents)
            .HasForeignKey(e => e.PlayerId);

        builder.Entity<MatchEvent>()
            .HasOne(e => e.Gameweek)
            .WithMany(g => g.MatchEvents)
            .HasForeignKey(e => e.GameweekId);

        builder.Entity<Transfer>()
            .HasOne(t => t.PlayerIn)
            .WithMany()
            .HasForeignKey(t => t.PlayerInId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transfer>()
            .HasOne(t => t.PlayerOut)
            .WithMany()
            .HasForeignKey(t => t.PlayerOutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeagueMember>()
            .HasOne(lm => lm.League)
            .WithMany(l => l.Members)
            .HasForeignKey(lm => lm.LeagueId);

        builder.Entity<LeagueMember>()
            .HasOne(lm => lm.User)
            .WithMany(u => u.LeagueMemberships)
            .HasForeignKey(lm => lm.UserId);

        builder.Entity<League>()
            .HasIndex(l => l.JoinCode)
            .IsUnique();

        builder.Entity<BundesligaPlayer>()
            .Property(p => p.Price)
            .HasPrecision(5, 1);

        builder.Entity<FantasyTeam>()
            .Property(t => t.Budget)
            .HasPrecision(5, 1);

        builder.Entity<Transfer>()
            .Property(t => t.PriceIn)
            .HasPrecision(5, 1);

        builder.Entity<Transfer>()
            .Property(t => t.PriceOut)
            .HasPrecision(5, 1);

        builder.Entity<Match>()
            .HasOne(m => m.Gameweek)
            .WithMany()
            .HasForeignKey(m => m.GameweekId);

        builder.Entity<Gameweek>()
            .Ignore(g => g.IsLocked)
            .Ignore(g => g.Deadline);

        // ---- DraftPick: enforces one-owner-per-player within a draft league ----
        builder.Entity<DraftPick>()
            .HasOne(p => p.League)
            .WithMany(l => l.DraftPicks)
            .HasForeignKey(p => p.LeagueId);

        builder.Entity<DraftPick>()
            .HasOne(p => p.Player)
            .WithMany()
            .HasForeignKey(p => p.PlayerId);

        builder.Entity<DraftPick>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

        // The critical constraint: a player can only be drafted once per league.
        builder.Entity<DraftPick>()
            .HasIndex(p => new { p.LeagueId, p.PlayerId })
            .IsUnique();

        // Each pick number is unique within a league (no two picks at slot #5).
        builder.Entity<DraftPick>()
            .HasIndex(p => new { p.LeagueId, p.PickNumber })
            .IsUnique();

        // ---- WaiverClaim ----
        builder.Entity<WaiverClaim>()
            .HasOne(c => c.League)
            .WithMany()
            .HasForeignKey(c => c.LeagueId);

        builder.Entity<WaiverClaim>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);

        builder.Entity<WaiverClaim>()
            .HasOne(c => c.Gameweek)
            .WithMany()
            .HasForeignKey(c => c.GameweekId);

        builder.Entity<WaiverClaim>()
            .HasOne(c => c.PlayerIn)
            .WithMany()
            .HasForeignKey(c => c.PlayerInId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WaiverClaim>()
            .HasOne(c => c.PlayerOut)
            .WithMany()
            .HasForeignKey(c => c.PlayerOutId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for fast queue lookups.
        builder.Entity<WaiverClaim>()
            .HasIndex(c => new { c.LeagueId, c.GameweekId, c.UserId, c.Priority });
    }
}
