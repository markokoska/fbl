using FBL.Api.Data;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

/// <summary>
/// Polls for draft picks whose deadline has elapsed and auto-picks for the user
/// who timed out. Sleeps cheaply when no draft is active so we don't hammer the
/// DB or spam logs.
/// </summary>
public class DraftAutoPickService : BackgroundService
{
    private static readonly TimeSpan FastInterval = TimeSpan.FromSeconds(2);   // when at least one draft is active
    private static readonly TimeSpan IdleInterval = TimeSpan.FromSeconds(30);  // when nothing is in progress

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DraftAutoPickService> _logger;

    public DraftAutoPickService(IServiceScopeFactory scopeFactory, ILogger<DraftAutoPickService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DraftAutoPickService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan nextDelay = IdleInterval;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Cheap probe: are there any drafts currently running? If not, idle.
                bool anyActive = await db.Leagues
                    .AnyAsync(l => l.Type == LeagueType.Draft && l.DraftStatus == DraftStatus.InProgress, stoppingToken);

                if (anyActive)
                {
                    var draftService = scope.ServiceProvider.GetRequiredService<DraftService>();
                    await draftService.ProcessExpiredAutoPicks();
                    nextDelay = FastInterval;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-pick sweep failed; will retry next tick");
            }

            try { await Task.Delay(nextDelay, stoppingToken); }
            catch (TaskCanceledException) { /* shutting down */ }
        }
    }
}
