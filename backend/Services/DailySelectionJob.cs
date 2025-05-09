using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Services; // Namespace for IDailySelectionService
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

    //TODO: Ændre til udkommenteret for live run. Test-version forneden
    /*
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
    */
    //* Sat til at køre job når der bruges 'dotnet run'. Bruges til TESTING!!
    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Selection Job starting.");

        // MIDLERTIDIGT TIL TEST: Kør med det samme (eller efter få sekunder)
        var initialDelay = TimeSpan.FromSeconds(5); // Kør om 5 sekunder
        // var initialDelay = TimeSpan.Zero; // Kør med det samme

        // MIDLERTIDIGT TIL TEST: Sæt evt. perioden til noget kort, f.eks. hvert minut, hvis du vil teste flere gange
        // TimeSpan period = TimeSpan.FromMinutes(1);
        // Husk at ændre tilbage til TimeSpan.FromHours(24) senere!
        TimeSpan period = TimeSpan.FromHours(24); // Normal periode

        _timer = new Timer(DoWork, null, initialDelay, period);

        _logger.LogInformation("Daily Selection Job timer scheduled.");

        // Overvej at køre én gang ved opstart uanset hvad, hvis data mangler?
        // CheckAndRunInitialSelectionAsync(); // Implementer evt. denne logik

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
                var dailySelectionService =
                    scope.ServiceProvider.GetRequiredService<IDailySelectionService>();
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
        _logger.LogInformation(
            "Daily Selection Job finished. Next run scheduled in approximately 24 hours."
        );
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
