namespace backend.Services.Calendar.Scraping;

using System;
using System.Threading;
using System.Threading.Tasks;
using backend.utils.TimeZone;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class AltingetScraperServiceScheduler : BackgroundService
{
    private readonly ILogger<AltingetScraperServiceScheduler> _logger;
    private readonly IServiceScopeFactory _scopeFactory; // Factory to create scopes for scoped services like DbContext
    private readonly ITimeZoneHelper _timeZoneHelper;

    // Inject logger, scope factory, and ITimeZoneHelper
    public AltingetScraperServiceScheduler(
        ILogger<AltingetScraperServiceScheduler> logger,
        IServiceScopeFactory scopeFactory,
        ITimeZoneHelper timeZoneHelper
    )
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _timeZoneHelper = timeZoneHelper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Altinget Scrape Service is starting.");

        // Loop until the application is stopped
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                TimeSpan delay = CalculateNextRunDelay();
                _logger.LogInformation("Next Altinget scrape will be in {Delay}.", delay);

                // Wait for the calculated delay
                await WaitForNextScheduledRunAsync(delay, stoppingToken);

                // --- Time to run the task ---
                _logger.LogInformation("Running scheduled Altinget scrape...");

                // Create a new DI scope to resolve scoped services (DataContext, AltingetScraperService)
                using (var scope = _scopeFactory.CreateScope())
                {
                    var scraperService =
                        scope.ServiceProvider.GetRequiredService<IScraperService>();
                    try
                    {
                        // Execute the automation task
                        int count = await scraperService.RunScraper();
                        _logger.LogInformation(
                            "Scheduled Altinget scrape finished. Processed {Count} events.",
                            count
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log errors specifically from the RunAutomation task
                        _logger.LogError(
                            ex,
                            "Error occurred during the execution of the automation task via IAutomationService."
                        );
                    }
                }
                _logger.LogInformation("Finished current scheduled scrape run.");

                // A small delay to prevent immediate re-calculation if the task finished exactly on time
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when the application is stopping
                _logger.LogInformation(
                    "Scheduled Altinget Scrape Service is stopping (Task Canceled)."
                );
                break; // Exit the loop
            }
            catch (TimeZoneNotFoundException tzEx)
            {
                _logger.LogCritical(
                    tzEx,
                    "CRITICAL ERROR: Copenhagen timezone not found. Scraper cannot run. Ensure OS supports 'Europe/Copenhagen' (Linux/macOS) or 'Central European Standard Time' (Windows)."
                );
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait an hour before trying again
            }
            catch (Exception ex)
            {
                // Catch unexpected errors in the scheduling loop itself
                _logger.LogError(ex, "Unexpected error in Scheduled Altinget Scrape Service loop.");
                // Wait before retrying to avoid tight loops on persistent errors
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        } // End while loop

        _logger.LogInformation("Scheduled Altinget Scrape Service has stopped.");
    }

    // Method to calculate the next run delay
    // This is separated for clarity and easier diagramming
    private TimeSpan CalculateNextRunDelay()
    {
        // --- Calculate Delay until next 4:00 AM Copenhagen time ---
        TimeZoneInfo copenhagenZone = _timeZoneHelper.FindTimeZone();
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtc, copenhagenZone); // Get current time in Copenhagen timezone
        DateTimeOffset nextRunTimeCopenhagen;

        if (nowCopenhagen.TimeOfDay >= TimeSpan.FromHours(4))
        {
            // If current time is 4 AM or later, schedule for 4 AM next day
            nextRunTimeCopenhagen = nowCopenhagen.Date.AddDays(1).AddHours(4);
        }
        else
        {
            // If current time is before 4 AM, schedule for 4 AM today
            nextRunTimeCopenhagen = nowCopenhagen.Date.AddHours(4);
        }

        TimeSpan delay = nextRunTimeCopenhagen - nowCopenhagen;

        if (delay < TimeSpan.Zero)
        {
            // This case should ideally not be hit if logic is correct, but as a fallback:
            delay = delay.Add(TimeSpan.FromDays(1)); // Add 24 hours if calculated delay is negative
        }
        return delay;
    }

    // --- Time Handling ---
    // This method is used to wait for the next scheduled run
    private async Task WaitForNextScheduledRunAsync(TimeSpan delay, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Waiting for {Delay} until the next scheduled run.", delay);
        await Task.Delay(delay, stoppingToken);
    }
}
