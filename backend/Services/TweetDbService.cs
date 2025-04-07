using backend.Data;       // Adgang til DataContext
using backend.Services;   // Adgang til TwitterService
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // Godt at have til logging
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For ToListAsync etc.

namespace backend.Services

{
    public class TweetFetchingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TweetFetchingService> _logger;
        // Gør intervallet konfigurerbart - hent evt. fra IConfiguration
        private readonly TimeSpan _period = TimeSpan.FromHours(24);

        public TweetFetchingService(IServiceScopeFactory scopeFactory, ILogger<TweetFetchingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TweetFetchingService starting.");

            // Vent lidt før første kørsel for at lade appen starte helt op
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("TweetFetchingService executing task at: {time}", DateTimeOffset.Now);

                try
                {
                    // Opret et nyt scope for at hente scoped services (DataContext, TwitterService)
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                        var twitterService = scope.ServiceProvider.GetRequiredService<TwitterService>();

                        // 1. Hent alle politiker-ID'er fra databasen
                        var politicians = await dbContext.PoliticianTwitterIds
                                                         .AsNoTracking()
                                                         .ToListAsync(stoppingToken); // Husk using Microsoft.EntityFrameworkCore;

                        _logger.LogInformation("Found {Count} politicians to fetch tweets for.", politicians.Count);

                        // 2. Loop igennem hver politiker og hent tweets
                        foreach (var politician in politicians)
                        {
                            if (stoppingToken.IsCancellationRequested) break; // Stop hvis appen lukker

                            try
                            {
                                _logger.LogInformation("Fetching tweets for {Name} ({TwitterUserId})...", politician.Name, politician.TwitterUserId);
                                // Kald din service til at hente OG gemme nye tweets
                                var newTweets = await twitterService.GetStructuredTweets(politician.TwitterUserId, 10); // Hent f.eks. 10 seneste
                                _logger.LogInformation("Fetched {Count} new tweets for {Name}.", newTweets.Count, politician.Name);

                                // Tilføj evt. en lille pause mellem hver politiker for ikke at hamre API'et for hårdt?
                                await Task.Delay(TimeSpan.FromMinutes(16), stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error fetching tweets for politician {Name} ({TwitterUserId})", politician.Name, politician.TwitterUserId);
                                // Fortsæt til næste politiker selvom én fejler
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred in TweetFetchingService task.");
                }

                // 3. Vent til næste kørsel (håndterer 15 min cooldown)
                _logger.LogInformation("TweetFetchingService task finished. Waiting for {Period} before next run.", _period);
                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("TweetFetchingService stopping.");
        }
    }
}