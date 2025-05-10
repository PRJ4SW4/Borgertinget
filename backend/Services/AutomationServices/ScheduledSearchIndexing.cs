// Services/AutomationServices/ScheduledIndexService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Services; // Where SearchIndexingService lives
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Services.AutomationServices // Adjust namespace if needed
{
    public class ScheduledIndexService : BackgroundService
    {
        private readonly ILogger<ScheduledIndexService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Factory to create scopes

        public ScheduledIndexService(
            ILogger<ScheduledIndexService> logger,
            IServiceScopeFactory scopeFactory
        )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled Search Indexing Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // --- Calculate Delay until next midnight Copenhagen time ---
                    TimeZoneInfo copenhagenZone = FindTimeZone(); // Re-use helper
                    DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
                    DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtc, copenhagenZone);

                    // Target time is midnight (start of the day)
                    DateTime targetTimeToday = nowCopenhagen.Date; // Midnight today
                    targetTimeToday.AddHours(4); // 4 a.m
                    targetTimeToday.AddMinutes(5); // 4:05 a.m (letting altinget scraper finish it's scheduled service )

                    // Determine the next run time (4 a.m tonight or  tomorrow)
                    DateTime nextRunTimeLocal;
                    if (nowCopenhagen.TimeOfDay >= TimeSpan.Zero) // If it's past midnight already today
                    {
                        // Schedule for midnight tomorrow
                        nextRunTimeLocal = targetTimeToday.AddDays(1);
                    }
                    else
                    {
                        // Should not happen if check is TimeOfDay >= TimeSpan.Zero
                        // but logically, if it was before midnight, schedule for today's midnight
                        // This case is technically covered by the AddDays(1) above
                        nextRunTimeLocal = targetTimeToday;
                    }

                    // Convert the scheduled local run time into a DateTimeOffset
                    DateTimeOffset nextRunTimeZoned = new DateTimeOffset(
                        nextRunTimeLocal,
                        copenhagenZone.GetUtcOffset(nextRunTimeLocal)
                    );

                    TimeSpan delay = nextRunTimeZoned - nowUtc;

                    // Ensure delay is non-negative (handles edge cases around DST changes near midnight)
                    if (delay < TimeSpan.Zero)
                    {
                        _logger.LogWarning(
                            "Calculated negative delay ({Delay}), setting to zero. This might happen briefly around time changes.",
                            delay
                        );
                        delay = TimeSpan.Zero;
                    }

                    _logger.LogInformation(
                        "Next search index run scheduled for: {TargetRunTime} Copenhagen time ({TargetRunTimeUtc} UTC). Waiting for {Delay}.",
                        nextRunTimeLocal.ToString("yyyy-MM-dd HH:mm:ss"),
                        nextRunTimeZoned.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        delay
                    );

                    // Wait for the calculated delay
                    await Task.Delay(delay, stoppingToken);

                    // --- Time to run the task ---
                    _logger.LogInformation("Running scheduled search indexing...");

                    // Create a DI scope to resolve scoped services (DataContext, SearchIndexingService)
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var indexingService =
                            scope.ServiceProvider.GetRequiredService<SearchIndexingService>();
                        try
                        {
                            // Execute the indexing task, passing the stopping token
                            await indexingService.RunFullIndexAsync(stoppingToken);
                            _logger.LogInformation(
                                "Scheduled search indexing finished successfully."
                            );
                        }
                        catch (OperationCanceledException)
                            when (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation(
                                "Search indexing task was cancelled during execution."
                            );
                            // Allow the outer loop to break gracefully
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Error occurred during the execution of SearchIndexingService.RunFullIndexAsync."
                            );
                            // Logged the error, loop will continue for the next day
                        }
                    }
                    _logger.LogInformation("Finished current scheduled index run.");

                    // Optional small delay to prevent tight loop if task finishes *exactly* at midnight
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation(
                        "Scheduled Search Indexing Service is stopping (Task Canceled)."
                    );
                    break; // Exit the loop
                }
                catch (TimeZoneNotFoundException tzEx)
                {
                    _logger.LogCritical(
                        tzEx,
                        "CRITICAL ERROR: Copenhagen timezone not found. Indexing service cannot run."
                    );
                    // Stop the service or wait longer
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Scheduled Index Service loop.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retrying
                }
            } // End while loop

            _logger.LogInformation("Scheduled Search Indexing Service has stopped.");
        }

        // Helper to find the timezone reliably (copied from ScheduledAltingetScrapeService)
        private TimeZoneInfo FindTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
            } // IANA ID
            catch (TimeZoneNotFoundException) { }
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

    public class TestScheduledIndexService : BackgroundService
    {
        private readonly ILogger<ScheduledIndexService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public TestScheduledIndexService(
            ILogger<ScheduledIndexService> logger,
            IServiceScopeFactory scopeFactory
        )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Scheduled Search Indexing Service is starting (TEST MODE - Running shortly after startup)."
            );

            // --- TEMPORARY: Give the app a few seconds to start up before the first run ---
            // In a real scenario outside initial testing, you might remove this first delay
            // if the logic inside the loop handles the first run correctly.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation(
                    "Scheduled Search Indexing Service stopped during initial delay."
                );
                return; // Exit if stopped before first run
            }
            // --- END TEMPORARY ---


            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan delay; // Declare delay variable

                try
                {
                    // ==============================================================
                    // --- MODIFICATION FOR TESTING: Run every ~30 seconds ---
                    // ==============================================================
                    // COMMENT OUT or REMOVE the original midnight calculation block:
                    /*
                    TimeZoneInfo copenhagenZone = FindTimeZone();
                    DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
                    DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtc, copenhagenZone);
                    DateTime targetTimeToday = nowCopenhagen.Date;
                    DateTime nextRunTimeLocal;
                    if (nowCopenhagen.TimeOfDay >= TimeSpan.Zero) {
                        nextRunTimeLocal = targetTimeToday.AddDays(1);
                    } else {
                        nextRunTimeLocal = targetTimeToday;
                    }
                    DateTimeOffset nextRunTimeZoned = new DateTimeOffset(nextRunTimeLocal, copenhagenZone.GetUtcOffset(nextRunTimeLocal));
                    delay = nextRunTimeZoned - nowUtc;
                    if (delay < TimeSpan.Zero) { delay = TimeSpan.Zero; }
                    _logger.LogInformation("Next search index run scheduled for: {TargetRunTime} ...", ...);
                    */

                    // INSTEAD, use a short fixed delay for testing:
                    delay = TimeSpan.FromSeconds(300000); // Run approximately every 15 seconds
                    _logger.LogInformation(
                        "TEST MODE: Indexing will run after a {Delay} delay.",
                        delay
                    );
                    // ==============================================================
                    // --- END MODIFICATION ---
                    // ==============================================================

                    // Wait for the (short) delay
                    await Task.Delay(delay, stoppingToken);

                    // --- Time to run the task ---
                    _logger.LogInformation("Running scheduled search indexing (TEST MODE)...");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var indexingService =
                            scope.ServiceProvider.GetRequiredService<SearchIndexingService>();
                        try
                        {
                            await indexingService.RunFullIndexAsync(stoppingToken);
                            _logger.LogInformation(
                                "Scheduled search indexing finished successfully (TEST MODE run)."
                            );
                        }
                        // ... (keep existing inner catch blocks for OperationCanceledException and general Exception) ...
                        catch (OperationCanceledException)
                            when (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation(
                                "Search indexing task was cancelled during execution."
                            );
                            throw; // Re-throw to stop the loop
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Error occurred during the execution of SearchIndexingService.RunFullIndexAsync."
                            );
                        }
                    }
                    _logger.LogInformation("Finished current scheduled index run (TEST MODE).");
                }
                // ... (keep existing outer catch blocks for TaskCanceledException, TimeZoneNotFoundException, Exception) ...
                catch (TaskCanceledException)
                {
                    _logger.LogInformation(
                        "Scheduled Search Indexing Service is stopping (Task Canceled)."
                    );
                    break; // Exit the loop
                }
                catch (TimeZoneNotFoundException tzEx) // Keep this in case you revert the change
                {
                    _logger.LogCritical(
                        tzEx,
                        "CRITICAL ERROR: Copenhagen timezone not found. Indexing service cannot run reliably."
                    );
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Scheduled Index Service loop.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Shorter delay in test mode on error
                }
            } // End while loop

            _logger.LogInformation("Scheduled Search Indexing Service has stopped.");
        }
    }
}
