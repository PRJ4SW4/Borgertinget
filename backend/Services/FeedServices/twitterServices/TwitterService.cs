using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using backend.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace backend.Services
{
    public class TwitterService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _bearerToken;
        private readonly DataContext _dbContext;

        public TwitterService(HttpClient httpClient, IConfiguration config, DataContext dbContext)
        {
            _httpClient = httpClient;
            _bearerToken =
                config["TwitterApi:BearerToken"]
                ?? throw new ArgumentNullException("TwitterApi:BearerToken not configured");
            _dbContext = dbContext;
        }

        // for nemmere forståelse vil jeg sætte model for PoliticianTwitterId i kommentar her
        /*
        public class PoliticianTwitterId
        {
            public int Id { get; set; }
            public string TwitterUserId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string TwitterHandle { get; set; } = string.Empty;
           
            public int? AktorId { get; set; }
        
            public List<Tweet> Tweets { get; set; } = new();
            public List<Subscription> Subscriptions { get; set; } = new();
            public virtual Aktor? Aktor { get; set; }
        
            
            public virtual List<Poll> Polls { get; set; } = new List<Poll>();
        */

        // Herunder, vil jeg forklare step by step, hvad der sker i koden.
        // først søges db for user id, som er baseret på TwitterUserId fra modlen PoliticianTwitterId
        // hvis den ikke findes, så returneres 0 og der printes en fejlmeddelelse i konsollen.

        public async Task<int> GetStructuredTweets(string userId, int count = 10)
        {
            //  Først tjekker vi, om userId er null eller tom
            var politician = await _dbContext
                .PoliticianTwitterIds.AsNoTracking()
                .FirstOrDefaultAsync(p => p.TwitterUserId == userId);

            if (politician == null)
            {
                Console.WriteLine(
                    $"Warning: Politician with Twitter User ID {userId} not found in database. Skipping tweet fetch."
                );
                return 0;
            }
            // hvis ikke den er null eller tom, så henter vi den twitterId, der matcher userId fra databasen.

            // 2. Hent eksisterende tweets fra databasen baseret på det interne db id, der passer med  PoliticianTwitterId, hvor den tjekker for twitterTweetId
            // alle de fundne tweets, bliver gemt i en hashmap, så vi kan tjekke dem senere hurtigt.
            int politicianRecordId = politician.Id;
            HashSet<string> existingTweetIds = new HashSet<string>();
            try
            {
                existingTweetIds = await _dbContext
                    .Tweets.Where(t => t.PoliticianTwitterId == politicianRecordId)
                    .Select(t => t.TwitterTweetId)
                    .ToHashSetAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error fetching existing tweet IDs for Politician DB ID {politicianRecordId}: {ex.Message}"
                );
                return 0;
            }

            // 3. Byg URL og hent fra Twitter API
            string url =
                $"https://api.twitter.com/2/users/{userId}/tweets"
                + // endpoint for twitter Api v2
                $"?max_results={count}"
                + // maksimalt antal tweets, som er defineret i parameteren count
                $"&expansions=attachments.media_keys"
                + // fortæller api, at vi vil have flere ting med når vi henter fra api
                $"&media.fields=preview_image_url,url,type"
                + // hvad vi vil hente, når vi henter fra api
                $"&tweet.fields=id,created_at,entities,public_metrics"; // hvad vi vil have med i svaret fra api tweetid, created_at, entities, likes og kommentar.

            //twitter API kræver OAuth 2.0 Bearer token autentifikation defor skal vi tilføje det til headeren i vores httpClient.
            // det er gemt i appsettings.json filen, og hentes fra konfigurationen.(skal ændres til env.)
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

            // herunder er forspørgselen til api'en, og vi fanger fejl, hvis det ikke er muligt..
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error: HTTP request failed when fetching tweets for Twitter User ID {userId}: {ex.Message}"
                );
                return 0;
            }

            // 4. Tjek API svar status, da twitter API kan returnere forskellige fejlmeddelelser, vil vores errorhandling, returnere errror beskeden fra api'en.
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"Error: Fetching tweets for Twitter User ID {userId} failed. Status: {response.StatusCode}, Response: {errorContent}"
                );
                return 0;
            }

            // 5. Hent JSON svaret fra API'et og parse det, da twitter sender en rå json svar til servicen, skal vi parse den
            var jsonResponse = await response.Content.ReadAsStringAsync();
            JObject json;
            try
            {
                json = JObject.Parse(jsonResponse); // Parse JSON svaret til JObject for at kunne navigere i responsen
            }
            catch (Exception ex) // Håndtering JSON parse fejl
            {
                Console.WriteLine(
                    $"Error: Failed to parse JSON response for Twitter User ID {userId}: {ex.Message}"
                );
                return 0;
            }

            var tweets = json["data"]; // til at gemme tweets fra json svaret
            var media = json["includes"]?["media"]; // til at gemme  media fra json svaret, hvis det findes.
            var newTweetEntities = new List<Tweet>(); // liste oprettes til at gemme nye tweets, der senere skal gemmes i databasen, da vi ikke vil gemme dem 1 efter 1.

            // 6. Behandling af tweets fra API svaret, så de senere hen kan gemmes i databasen.
            //først tjekker vi om tweets er null, hvis det er tilfældet, så returnerer vi 0 og printer en fejlmeddelelse i konsollen.
            // hvis tweet id'et er null eller allerede findes i databasen, så springer vi det over, på den måde undgår vi at gemme det samme tweet flere gange.
            if (tweets != null)
            {
                foreach (var tweet in tweets)
                {
                    string? twitterTweetId = tweet["id"]?.ToString();

                    if (
                        string.IsNullOrEmpty(twitterTweetId)
                        || existingTweetIds.Contains(twitterTweetId)
                    )
                    {
                        continue;
                    }

                    // når vi når dette step, vil der derfor kun være nye tweets, som ikke findes i databasen.
                    string text = tweet["text"]?.ToString() ?? ""; // tweet teksten hentes fra json svaret, hvis den ikke findes, så sætter vi den til tom.
                    text = System
                        .Text.RegularExpressions.Regex.Replace(text, @"https?:\/\/t\.co\/\S+", "")
                        .Trim(); // fjerner specielt det link fra teksten ved hjælp af regex,

                    string mediaUrl = "";

                    //Mediea ekstraktion
                    // Først forsøger vi at finde direkte vedhæftede medier i tweetet
                    var mediaKeys = tweet["attachments"]?["media_keys"]; // Henter media_keys fra tweet
                    if (mediaKeys != null && media != null)
                    {
                        foreach (var key in mediaKeys) // For hver media_key i tweetet
                        {
                            // Find det matchende medie-objekt i includes.media sektionen
                            var matchedMedia = media.FirstOrDefault(m =>
                                m["media_key"]?.ToString() == key?.ToString()
                            );
                            if (matchedMedia != null)
                            {
                                // Hent enten fuld URL eller preview URL
                                // url bruges for billeder, preview_image_url bruges for videoer
                                mediaUrl =
                                    matchedMedia["url"]?.ToString()
                                    ?? matchedMedia["preview_image_url"]?.ToString()
                                    ?? "";
                                break; // Stop efter første medie er fundet.
                            }
                        }
                    }

                    // Hvis ingen direkte medier blev fundet i tweetet, prøver vi at hente billede fra første link, disse er billede fra andre url, derfor skal vi skrabe billedet fra websiden.
                    if (string.IsNullOrEmpty(mediaUrl))
                    {
                        // Find første URL i tweetet ved at bruge entities.urls array
                        var link = tweet["entities"]
                            ?["urls"]?.FirstOrDefault()
                            ?["expanded_url"]?.ToString();
                        if (!string.IsNullOrEmpty(link))
                        {
                            // Skrab OpenGraph billede-tag fra websiden på linkets destination
                            var ogImage = await GetOpenGraphImageAsync(link); // Kalder hjælpemetode der skraber HTML
                            if (!string.IsNullOrEmpty(ogImage))
                                mediaUrl = ogImage; // Brug OpenGraph billedet hvis det blev fundet
                        }
                    }

                    // DE metrics vi henter, er likes, retweets og replies.
                    var metrics = tweet["public_metrics"];
                    int likes = metrics?["like_count"]?.ToObject<int>() ?? 0;
                    int retweets = metrics?["retweet_count"]?.ToObject<int>() ?? 0;
                    int replies = metrics?["reply_count"]?.ToObject<int>() ?? 0;
                    DateTime createdAt =
                        tweet["created_at"]?.ToObject<DateTime>() ?? DateTime.UtcNow;

                    // Opretter et DB Entity for det nye tweet
                    var tweetEntity = new Tweet
                    {
                        TwitterTweetId = twitterTweetId,
                        Text = text,
                        ImageUrl = mediaUrl,
                        Likes = likes,
                        Retweets = retweets,
                        Replies = replies,
                        PoliticianTwitterId = politicianRecordId,
                        CreatedAt = createdAt,
                    };
                    // putter det i listen, det nævntes før, at vi ikke gemmer dem 1 efter.
                    newTweetEntities.Add(tweetEntity);
                }
            }

            // 7. Gem alle nye tweet entities til databasen
            if (newTweetEntities.Any())
            {
                int newTweetsCount = newTweetEntities.Count; // Gem antallet Før SaveChanges
                try
                {
                    _dbContext.Tweets.AddRange(newTweetEntities);
                    await _dbContext.SaveChangesAsync();
                    return newTweetsCount; // Returner antallet hvis SaveChanges lykkedes
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(
                        $"Error: Failed saving new tweets to database for Politician DB ID {politicianRecordId}: {ex.Message}"
                    );
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        // websraber, ti hvis der er link til et billede, som vi jo også gerne vil vise i vores feed
        // besøger vi linket og kigger efter et OpenGraph billede som websiden har defineret.
        private async Task<string?> GetOpenGraphImageAsync(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Brug en beskrivende User-Agent
                request.Headers.Add(
                    "User-Agent",
                    "Mozilla/5.0 (compatible; PoliticianTweetFetcher/1.0; +http://yourdomain.com/botinfo)"
                );

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null; // Returner null hvis siden ikke kan hentes

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                // Find specifikt 'og:image' meta tag
                var metaTag = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                // Returner 'content' attributten hvis tag'et findes
                return metaTag?.GetAttributeValue("content", null!);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error: Failed to scrape OpenGraph image from {url}: {ex.Message}"
                );
                return null;
            }
        }
    }
}
