using backend.Data;       
using backend.Services;   
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; /
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 


namespace backend.Services 
    public class TweetFetchingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TweetFetchingService> _logger;
        
        private readonly TimeSpan _period = TimeSpan.FromHours(24);
        //private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

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
                _logger.LogInformation("TweetFetchingService executing task cycle at: {time}", DateTimeOffset.Now);

                try
                {
                    // Opret et scope for denne kørsel
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                        var twitterService = scope.ServiceProvider.GetRequiredService<TwitterService>();

                        // 1. Hent alle politikere
                        var politicians = await dbContext.PoliticianTwitterIds
                                                         .AsNoTracking()
                                                         .ToListAsync(stoppingToken);

                        _logger.LogInformation("Found {Count} politicians to fetch tweets for.", politicians.Count);

                        // 2. Loop igennem og hent/gem tweets
                        foreach (var politician in politicians)
                        {
                            // Tjek om der er anmodet om stop før vi starter på en politiker
                            if (stoppingToken.IsCancellationRequested) break;

                            try
                            {
                                _logger.LogInformation("Fetching and saving tweets for {Name} ({TwitterUserId})...", politician.Name, politician.TwitterUserId);

                                // Kald service - forventer nu int retur (antal gemte)
                                int savedTweetsCount = await twitterService.GetStructuredTweets(politician.TwitterUserId, 10);

                                // Log antallet gemt
                                _logger.LogInformation("Saved {Count} new tweets for {Name}.", savedTweetsCount, politician.Name);

                                // *** GENINDSAT PAUSE PÅ 16 MINUTTER HER ***
                                // Nødvendigt pga. observeret stram rate limit (HTTP 429 fejl).
                                // Tjekker igen for stop-signal før den lange pause.
                                if (!stoppingToken.IsCancellationRequested)
                                {
                                    _logger.LogInformation("Waiting 16 minutes before fetching next politician due to rate limits...");
                                    await Task.Delay(TimeSpan.FromMinutes(16), stoppingToken);
                                }

                            }
                            catch (Exception ex)
                            {
                                // Log fejl for specifik politiker, men fortsæt loopet
                                _logger.LogError(ex, "Error processing tweets for politician {Name} ({TwitterUserId})", politician.Name, politician.TwitterUserId);
                                // Overvej stadig en pause her ELLER bedre fejlhåndtering af 429 specifikt?
                                // For nu fortsætter den til næste politiker efter logning.
                                // En pause her kunne også være relevant:
                                // if (!stoppingToken.IsCancellationRequested)
                                // {
                                //     _logger.LogWarning("Waiting 16 minutes after error for politician {Name} to allow cooldown.", politician.Name);
                                //     await Task.Delay(TimeSpan.FromMinutes(16), stoppingToken);
                                // }
                            }
                        } // Slut på foreach politiker
                    } // Slut på using scope
                }
                catch (Exception ex)
                {
                    // Log uventede fejl i hele task-cyklussen
                    _logger.LogError(ex, "Unhandled exception occurred in TweetFetchingService task loop.");
                }

                // 3. Vent 24 timer før næste cyklus (denne pause starter EFTER alle politikere er behandlet)
                // Tjekker igen for stop-signal før den lange pause.
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("TweetFetchingService task cycle finished. Waiting for {Period} before next run.", _period);
                    await Task.Delay(_period, stoppingToken);
                }

            } // Slut på while (!stoppingToken.IsCancellationRequested)

            _logger.LogInformation("TweetFetchingService stopping.");
        } // Slut på ExecuteAsync
    } // Slut på klasse
} // Slut på namespace