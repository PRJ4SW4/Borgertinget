namespace backend.Services.Search
{
    public class ScheduledIndexService : BackgroundService
    {
        private readonly ILogger<ScheduledIndexService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

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
                    TimeZoneInfo copenhagenZone = FindTimeZone();
                    DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
                    DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtc, copenhagenZone);

                    // Target time is midnight (start of the day)
                    DateTime targetTimeToday = nowCopenhagen.Date;
                    targetTimeToday.AddHours(4);
                    targetTimeToday.AddMinutes(5);

                    // Determine the next run time (4:05 a.m tonight or  tomorrow)
                    DateTime nextRunTimeLocal;
                    if (nowCopenhagen.TimeOfDay >= TimeSpan.Zero)
                    {
                        // Schedule for midnight tomorrow
                        nextRunTimeLocal = targetTimeToday.AddDays(1);
                    }
                    else
                    {
                        nextRunTimeLocal = targetTimeToday;
                    }

                    // Convert the scheduled local run time into a DateTimeOffset
                    DateTimeOffset nextRunTimeZoned = new DateTimeOffset(
                        nextRunTimeLocal,
                        copenhagenZone.GetUtcOffset(nextRunTimeLocal)
                    );

                    TimeSpan delay = nextRunTimeZoned - nowUtc;

                    // Ensure delay is non-negative
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

                    // Create a DI scope to resolve scoped services
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
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Error occurred during the execution of SearchIndexingService.RunFullIndexAsync."
                            );
                        }
                    }
                    _logger.LogInformation("Finished current scheduled index run.");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation(
                        "Scheduled Search Indexing Service is stopping (Task Canceled)."
                    );
                    break;
                }
                catch (TimeZoneNotFoundException tzEx)
                {
                    _logger.LogCritical(
                        tzEx,
                        "CRITICAL ERROR: Copenhagen timezone not found. Indexing service cannot run."
                    );
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Scheduled Index Service loop.");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait before retrying
                }
            }

            _logger.LogInformation("Scheduled Search Indexing Service has stopped.");
        }

        // Helper to find the timezone reliably
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
}
