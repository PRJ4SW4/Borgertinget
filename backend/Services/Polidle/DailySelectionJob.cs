// Fil: Jobs/DailySelectionJob.cs
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using backend.Enums;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;
using backend.Interfaces.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Jobs
{
    public class DailySelectionJobSettings 
    {
        public string RunTimeUtc { get; set; } = "00:03";
        public double RunCheckIntervalMinutes { get; set; } = 5;
    }

    public class DailySelectionJob : IHostedService, IDisposable
    {
        private readonly ILogger<DailySelectionJob> _logger;
        private Timer? _timer = null;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly DailySelectionJobSettings _settings; 
        private readonly TimeSpan _checkInterval;

        private volatile bool _isExecuting = false;
        private readonly object _lock = new object();

        public DailySelectionJob(
            ILogger<DailySelectionJob> logger,
            IServiceScopeFactory scopeFactory,
            IDateTimeProvider dateTimeProvider, 
            IConfiguration configuration
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _dateTimeProvider =
                dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _settings =
                configuration.GetSection("DailySelectionJob").Get<DailySelectionJobSettings>()
                ?? new DailySelectionJobSettings();
            _checkInterval = TimeSpan.FromMinutes(_settings.RunCheckIntervalMinutes);
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Daily Selection Job starting up. Check interval: {CheckIntervalMinutes} minutes. Target Run Time (UTC): {RunTimeUtc}",
                _settings.RunCheckIntervalMinutes,
                _settings.RunTimeUtc
            );

            //* Start timer med det samme, men kørsel sker kun på det rigtige tidspunkt
            _timer = new Timer(DoWorkWrapper, null, TimeSpan.FromSeconds(15), _checkInterval); // Start efter 15 sek, tjek hvert X minut

            return Task.CompletedTask;
        }

        private void DoWorkWrapper(object? state)
        {
            // Simpel lås for at undgå at starte en ny kørsel, hvis den forrige stadig kører
            lock (_lock)
            {
                if (_isExecuting)
                {
                    _logger.LogWarning(
                        "Daily Selection Job timer triggered, but previous execution is still running. Skipping."
                    );
                    return;
                }
                _isExecuting = true;
            }

            _logger.LogInformation("Timer triggered. Checking if job should run.");
            // Kør selve arbejdet asynkront
            _ = DoWorkAsync();
        }

        private async Task DoWorkAsync()
        {
            bool jobExecuted = false;
            try
            {
                // Tjek om det er tid til at køre jobbet
                TimeSpan targetTime;
                if (
                    !TimeSpan.TryParseExact(
                        _settings.RunTimeUtc,
                        "hh\\:mm",
                        CultureInfo.InvariantCulture,
                        out targetTime
                    )
                )
                {
                    _logger.LogError(
                        "Invalid RunTimeUtc format in configuration: {RunTimeUtc}. Expected HH:mm.",
                        _settings.RunTimeUtc
                    );
                    return;
                }

                var now = _dateTimeProvider.UtcNow;
                var today = _dateTimeProvider.TodayUtc;

                // Skal vi køre i dag? Tjek om tidspunktet er passeret, OG om vi allerede HAR kørt i dag
                bool alreadyRunToday = await CheckIfRunTodayAsync(today);

                if (now.TimeOfDay >= targetTime && !alreadyRunToday)
                {
                    _logger.LogInformation(
                        "Daily Selection Job work starting for date {Date}.",
                        today
                    );
                    jobExecuted = true; // Markér at vi forsøger at køre

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dailySelectionService =
                            scope.ServiceProvider.GetRequiredService<IDailySelectionService>();
                        var markerRepository =
                            scope.ServiceProvider.GetRequiredService<IDailySelectionRepository>();

                        _logger.LogInformation(
                            "Calling SelectAndSaveDailyPoliticiansAsync for date {Date}",
                            today
                        );
                        await dailySelectionService.SelectAndSaveDailyPoliticiansAsync(today);
                        _logger.LogInformation(
                            "SelectAndSaveDailyPoliticiansAsync completed for date {Date}",
                            today
                        );
                    }
                }
                else
                {
                    if (alreadyRunToday)
                        _logger.LogInformation(
                            "Daily Selection Job check: Job has already run for {Date}.",
                            today
                        );
                    else
                        _logger.LogInformation(
                            "Daily Selection Job check: Target time {TargetTimeUtc} not reached yet (current time {CurrentTimeUtc}).",
                            targetTime,
                            now.TimeOfDay
                        );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred within the scoped execution of the Daily Selection Job work."
                );
            }
            finally
            {
                lock (_lock) // Frigiv låsen
                {
                    _isExecuting = false;
                }
                if (jobExecuted)
                    _logger.LogInformation(
                        "Daily Selection Job work finished for date {Date}.",
                        _dateTimeProvider.TodayUtc
                    );
                else
                    _logger.LogInformation("Daily Selection Job check finished.");
            }
        }

        // Helper til at tjekke om jobbet allerede er kørt
        private async Task<bool> CheckIfRunTodayAsync(DateOnly today)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var dailySelectionRepository =
                        scope.ServiceProvider.GetRequiredService<IDailySelectionRepository>();
                    bool exists = await dailySelectionRepository.ExistsForDateAsync(today);
                    if (exists)
                    {
                        _logger.LogDebug(
                            "CheckIfRunTodayAsync: Found existing DailySelections for {Date}.",
                            today
                        );
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to check if job has already run for {Date}.",
                        today
                    );
                    return false;
                }
            }
            return false;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Selection Job stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogDebug("Disposing Daily Selection Job timer.");
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
