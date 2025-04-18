// Services/AutomationServices/ScheduledAltingetScrapeService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceScopeFactory
using Microsoft.Extensions.Hosting; // Required for BackgroundService
using Microsoft.Extensions.Logging; // Required for ILogger

namespace backend.Services.AutomationServices // Adjust namespace if needed
{
    public class ScheduledAltingetScrapeService : BackgroundService
    {
        private readonly ILogger<ScheduledAltingetScrapeService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Factory to create scopes for scoped services like DbContext

        // Inject logger and scope factory
        public ScheduledAltingetScrapeService(
            ILogger<ScheduledAltingetScrapeService> logger,
            IServiceScopeFactory scopeFactory
        )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled Altinget Scrape Service is starting.");

            // Loop until the application is stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // --- Calculate Delay until next 4:00 AM Copenhagen time ---
                    TimeZoneInfo copenhagenZone = FindTimeZone(); // Helper to get timezone reliably
                    DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
                    DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtc, copenhagenZone);

                    // Target time is 4:00 AM on the current local date
                    DateTime targetTimeToday = nowCopenhagen.Date.AddHours(4);

                    // Determine the next run time (either 4 AM today or 4 AM tomorrow)
                    DateTime nextRunTimeLocal;
                    if (nowCopenhagen.TimeOfDay >= TimeSpan.FromHours(4))
                    {
                        // It's 4 AM or later today, schedule for 4 AM tomorrow
                        nextRunTimeLocal = targetTimeToday.AddDays(1);
                    }
                    else
                    {
                        // It's before 4 AM today, schedule for 4 AM today
                        nextRunTimeLocal = targetTimeToday;
                    }

                    // Convert the scheduled local run time into a DateTimeOffset using the correct zone offset
                    // This correctly handles potential DST transitions near 4 AM
                    DateTimeOffset nextRunTimeZoned = new DateTimeOffset(
                        nextRunTimeLocal,
                        copenhagenZone.GetUtcOffset(nextRunTimeLocal)
                    );

                    // Calculate the delay from the current UTC time until the target UTC time
                    TimeSpan delay = nextRunTimeZoned - nowUtc;

                    if (delay < TimeSpan.Zero)
                    {
                        // Should not happen with the logic above, but safety check
                        delay = TimeSpan.Zero;
                    }

                    _logger.LogInformation(
                        "Next Altinget scrape scheduled for: {TargetRunTime} Copenhagen time ({TargetRunTimeUtc} UTC). Waiting for {Delay}.",
                        nextRunTimeLocal.ToString("yyyy-MM-dd HH:mm:ss"),
                        nextRunTimeZoned.ToString("yyyy-MM-dd HH:mm:ss"),
                        delay
                    );

                    // Wait for the calculated delay, honoring the cancellation token
                    await Task.Delay(delay, stoppingToken);

                    // --- Time to run the task ---
                    _logger.LogInformation("Running scheduled Altinget scrape...");

                    // Create a new DI scope to resolve scoped services (DataContext, AltingetScraperService)
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var scraperService =
                            scope.ServiceProvider.GetRequiredService<AltingetScraperService>();
                        try
                        {
                            // Execute the automation task
                            int count = await scraperService.RunAutomation();
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
                                "Error occurred during the execution of AltingetScraperService.RunAutomation."
                            );
                        }
                    }
                    _logger.LogInformation("Finished current scheduled scrape run.");

                    // Optional: Add a small delay to prevent immediate re-calculation if the task finished exactly on time
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
                    // Optionally stop the service or wait longer before retrying
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait an hour before trying again
                }
                catch (Exception ex)
                {
                    // Catch unexpected errors in the scheduling loop itself
                    _logger.LogError(
                        ex,
                        "Unexpected error in Scheduled Altinget Scrape Service loop."
                    );
                    // Wait before retrying to avoid tight loops on persistent errors
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            } // End while loop

            _logger.LogInformation("Scheduled Altinget Scrape Service has stopped.");
        }

        // Helper to find the timezone reliably on different OS
        private TimeZoneInfo FindTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
            } // IANA ID (Linux/macOS)
            catch (TimeZoneNotFoundException) { } // Ignore and try Windows ID
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            } // Windows ID
            catch (TimeZoneNotFoundException ex)
            {
                _logger.LogCritical(
                    ex,
                    "Could not find Copenhagen timezone using either IANA or Windows ID."
                );
                throw; // Re-throw if neither is found
            }
        }
    }
}
