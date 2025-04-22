using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Services; // Namespace for IDailySelectionService

public class DailySelectionJob : IHostedService, IDisposable
{
    private readonly ILogger<DailySelectionJob> _logger;
    private Timer? _timer = null;
    private readonly IServiceScopeFactory _scopeFactory;

    public DailySelectionJob(ILogger<DailySelectionJob> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory; // Bruges til at få en scoped service (som DbContext)
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Selection Job starting.");

        // Beregn tid til næste kørsel (f.eks. kl 00:05 UTC)
        var now = DateTime.UtcNow;
        var nextRunTime = now.Date.AddDays(1).AddMinutes(5); // Næste dag kl 00:05 UTC
        var initialDelay = nextRunTime - now;
        if (initialDelay.TotalMilliseconds < 0) {
            initialDelay = TimeSpan.FromMinutes(1); // Kør om 1 min hvis tiden allerede er passeret
             _logger.LogWarning("Next run time {NextRunTime} is in the past. Running in 1 minute.", nextRunTime);
        }


        _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromHours(24)); // Kør nu, og så hver 24 timer

        // Overvej at køre én gang med det samme ved opstart hvis ingen data findes for i dag?
        // CheckAndRunInitialSelectionAsync();

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Daily Selection Job is running.");

        try
        {
             // Opret et scope for at få fat i scoped services som DbContext/IDailySelectionService
             using (var scope = _scopeFactory.CreateScope())
             {
                  var dailySelectionService = scope.ServiceProvider.GetRequiredService<IDailySelectionService>();
                  var today = DateOnly.FromDateTime(DateTime.UtcNow);

                  // Kald service metoden
                  await dailySelectionService.SelectAndSaveDailyPoliticiansAsync(today);
             }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "An error occurred in the Daily Selection Job.");
        }

         // Beregn næste kørselstid igen for en sikkerheds skyld (hvis server genstarter etc.)
         // Kan gøres mere robust, men TimeSpan.FromHours(24) er ofte ok.
         _logger.LogInformation("Daily Selection Job finished. Next run scheduled in approximately 24 hours.");
    }


    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Selection Job is stopping.");
        _timer?.Change(Timeout.Infinite, 0); // Stop timeren
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}