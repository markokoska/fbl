namespace FBL.Api.Services;

/// <summary>
/// Polls every couple of minutes and resolves any draft league whose
/// upcoming GW deadline is within 24h and hasn't yet processed waivers.
/// </summary>
public class WaiverProcessingService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WaiverProcessingService> _logger;

    public WaiverProcessingService(IServiceScopeFactory scopeFactory, ILogger<WaiverProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WaiverProcessingService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<WaiverService>();
                await svc.ProcessDueLeagues();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Waiver processing tick failed");
            }
            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { /* shutting down */ }
        }
    }
}
