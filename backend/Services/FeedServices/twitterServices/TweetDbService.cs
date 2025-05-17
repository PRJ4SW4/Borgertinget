using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// TweetDbService, er en service, der har til ansvar at kører TwitterService, i en strukturet, så vi på den måde undgår at komme i problemer med twitters 15 min cool down regl for free users.
// det der sker i koden, er at den fetcher hele listen af potiker i PollitianwitterIds table, og bruger disse id'er til at fetcher hver især, vente 16 min, hvorefter den fetcher den næste
// dett er selfølge ikke optimal, men dette var et lille workaround ift. det er proff of concept, så der ikke en grund til at betale for et twitter abbonement.
// efter at have fetchet alle politiker, vil den vente 24 timer, og så køre det hele igen.

namespace backend.Services // Eller f.eks. backend.BackgroundServices
{
    public class TweetFetchingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TweetFetchingService> _logger;

        // Interval sat til 1 gang i døgnet
        private readonly TimeSpan _period = TimeSpan.FromHours(24);

        public TweetFetchingService(
            IServiceScopeFactory scopeFactory,
            ILogger<TweetFetchingService> logger
        )
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
                _logger.LogInformation(
                    "TweetFetchingService executing task cycle at: {time}",
                    DateTimeOffset.Now
                );

                try
                {
                    // Opret et scope for denne kørsel
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                        var twitterService =
                            scope.ServiceProvider.GetRequiredService<TwitterService>();

                        // 1. Hent alle politikere
                        var politicians = await dbContext
                            .PoliticianTwitterIds.AsNoTracking()
                            .ToListAsync(stoppingToken);

                        _logger.LogInformation(
                            "Found {Count} politicians to fetch tweets for.",
                            politicians.Count
                        );

                        // 2. Loop igennem og hent/gem tweets
                        foreach (var politician in politicians)
                        {
                            // Tjek om der er anmodet om stop før vi starter på en politiker
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            try
                            {
                                _logger.LogInformation(
                                    "Fetching and saving tweets for {Name} ({TwitterUserId})...",
                                    politician.Name,
                                    politician.TwitterUserId
                                );

                                int savedTweetsCount = await twitterService.GetStructuredTweets(
                                    politician.TwitterUserId,
                                    10
                                );

                                // Log antallet gemt
                                _logger.LogInformation(
                                    "Saved {Count} new tweets for {Name}.",
                                    savedTweetsCount,
                                    politician.Name
                                );

                                if (!stoppingToken.IsCancellationRequested)
                                {
                                    _logger.LogInformation(
                                        "Waiting 16 minutes before fetching next politician due to rate limits..."
                                    );
                                    await Task.Delay(TimeSpan.FromMinutes(16), stoppingToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(
                                    ex,
                                    "Error processing tweets for politician {Name} ({TwitterUserId})",
                                    politician.Name,
                                    politician.TwitterUserId
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception occurred in TweetFetchingService task loop."
                    );
                }

                // 3. Vent 24 timer før næste cyklus

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "TweetFetchingService task cycle finished. Waiting for {Period} before next run.",
                        _period
                    );
                    await Task.Delay(_period, stoppingToken);
                }
            }

            _logger.LogInformation("TweetFetchingService stopping.");
        }
    }
}
