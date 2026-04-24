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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasOne(u => u.FantasyTeam)
            .WithOne(t => t.User)
            .HasForeignKey<FantasyTeam>(t => t.UserId);

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

        // Gameweek: store Deadline as a computed-like column, but since EF doesn't
        // support computed from C# property, we store KickoffTime and Deadline is C#-only.
        builder.Entity<Match>()
            .HasOne(m => m.Gameweek)
            .WithMany()
            .HasForeignKey(m => m.GameweekId);

        builder.Entity<Gameweek>()
            .Ignore(g => g.IsLocked)
            .Ignore(g => g.Deadline);
    }
}
