using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System; // Tilføjet for Console.WriteLine og Exception

namespace backend.Services
{
    public class TwitterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bearerToken;
        private readonly DataContext _dbContext;

        public TwitterService(HttpClient httpClient, IConfiguration config, DataContext dbContext)
        {
            _httpClient = httpClient;
            _bearerToken = config["TwitterApi:BearerToken"];
            _dbContext = dbContext;
        }

        // Denne metode henter tweets, tjekker for dubletter mod DB,
        // gemmer kun nye tweets, og returnerer DTO'er for de nye.
        public async Task<List<TweetDto>> GetStructuredTweets(string userId, int count = 10)
        {
            // 1. Find politikerens interne DB ID ud fra Twitter User ID
            var politician = await _dbContext.PoliticianTwitterIds
                                 .AsNoTracking() // Ingen grund til at tracke entity her
                                 .FirstOrDefaultAsync(p => p.TwitterUserId == userId);

            if (politician == null)
            {
                // Indikerer fejl - politiker ikke fundet i DB
                Console.WriteLine($"Warning: Politician with Twitter User ID {userId} not found in database. Skipping tweet fetch.");
                return new List<TweetDto>(); // Returner tom liste
            }
            int politicianRecordId = politician.Id; // Gem den interne DB Id

            // 2. Hent eksisterende TwitterTweetId'er for denne politiker
            HashSet<string> existingTweetIds = new HashSet<string>();
            try
            {
                 existingTweetIds = await _dbContext.Tweets
                                        .Where(t => t.PoliticianTwitterId == politicianRecordId)
                                        .Select(t => t.TwitterTweetId) // Kun ID'et er nødvendigt
                                        .ToHashSetAsync(); // Giver hurtigt Contains-tjek
            }
            catch (Exception ex)
            {
                 // Fejl under læsning fra DB
                 Console.WriteLine($"Error fetching existing tweet IDs for Politician DB ID {politicianRecordId}: {ex.Message}");
                 return new List<TweetDto>(); // Returner tom liste ved DB fejl
            }

            // 3. Byg URL og hent fra Twitter API
            // Husk at sikre, at 'id' og 'created_at' er med i tweet.fields
            string url = $"https://api.twitter.com/2/users/{userId}/tweets" +
                         $"?max_results={count}" +
                         $"&expansions=attachments.media_keys" +
                         $"&media.fields=preview_image_url,url,type" +
                         $"&tweet.fields=id,created_at,entities,public_metrics"; // id og created_at tilføjet explicit

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                 // Netværksfejl
                 Console.WriteLine($"Error: HTTP request failed when fetching tweets for Twitter User ID {userId}: {ex.Message}");
                 return new List<TweetDto>();
            }

            // 4. Tjek API svar status
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                // Fejl fra Twitter API (fx rate limit, forkert ID)
                 Console.WriteLine($"Error: Fetching tweets for Twitter User ID {userId} failed. Status: {response.StatusCode}, Response: {errorContent}");
                return new List<TweetDto>();
            }

            // 5. Parse JSON svar
            var jsonResponse = await response.Content.ReadAsStringAsync();
            JObject json;
             try
             {
                 json = JObject.Parse(jsonResponse);
             }
             catch (Exception ex) // Håndter JSON parse fejl
             {
                  Console.WriteLine($"Error: Failed to parse JSON response for Twitter User ID {userId}: {ex.Message}");
                  return new List<TweetDto>();
             }


            var tweets = json["data"];
            var media = json["includes"]?["media"];
            var tweetDtos = new List<TweetDto>();         // Liste til DTO'er for nye tweets
            var newTweetEntities = new List<Tweet>();   // Liste til Entities for nye tweets

            // 6. Behandl tweets fra API svaret
            if (tweets != null)
            {
                foreach (var tweet in tweets)
                {
                    // Hent Twitter's unikke ID for tweetet
                    string? twitterTweetId = tweet["id"]?.ToString();

                    // TJEK FOR DUBLET: Spring over hvis ID mangler eller allerede findes i DB
                    if (string.IsNullOrEmpty(twitterTweetId) || existingTweetIds.Contains(twitterTweetId))
                    {
                        continue; // Gå til næste tweet i API svaret
                    }

                    // --- Kun NYE tweets når hertil ---

                    // Uddrag data fra tweet (tekst, medier, metrics, timestamp)
                    string text = tweet["text"]?.ToString() ?? "";
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"https?:\/\/t\.co\/\S+", "").Trim();

                    string mediaUrl = "";
                    var mediaKeys = tweet["attachments"]?["media_keys"];
                    if (mediaKeys != null && media != null)
                    {
                         foreach (var key in mediaKeys)
                         {
                             var matchedMedia = media.FirstOrDefault(m => m["media_key"]?.ToString() == key?.ToString());
                             if (matchedMedia != null)
                             {
                                 mediaUrl = matchedMedia["url"]?.ToString() ?? matchedMedia["preview_image_url"]?.ToString() ?? "";
                                 break;
                             }
                         }
                    }

                    // Forsøg at finde OpenGraph billede hvis ingen direkte mediaUrl
                    if (string.IsNullOrEmpty(mediaUrl))
                    {
                        var link = tweet["entities"]?["urls"]?.FirstOrDefault()?["expanded_url"]?.ToString();
                        if (!string.IsNullOrEmpty(link))
                        {
                           // Overvej om dette kald altid er nødvendigt eller kan sløve processen
                           var ogImage = await GetOpenGraphImageAsync(link);
                           if (!string.IsNullOrEmpty(ogImage))
                               mediaUrl = ogImage;
                        }
                    }

                    var metrics = tweet["public_metrics"];
                    int likes = metrics?["like_count"]?.ToObject<int>() ?? 0;
                    int retweets = metrics?["retweet_count"]?.ToObject<int>() ?? 0;
                    int replies = metrics?["reply_count"]?.ToObject<int>() ?? 0;
                    DateTime createdAt = tweet["created_at"]?.ToObject<DateTime>() ?? DateTime.UtcNow;

                    // Opret DTO for det nye tweet
                    var tweetDto = new TweetDto
                    {
                        Text = text,
                        ImageUrl = mediaUrl,
                        Likes = likes,
                        Retweets = retweets,
                        Replies = replies
                        // Tilføj evt. CreatedAt eller TwitterTweetId hvis frontend skal bruge det
                    };
                    tweetDtos.Add(tweetDto);

                    // Opret DB Entity for det nye tweet
                    var tweetEntity = new Tweet
                    {
                        TwitterTweetId = twitterTweetId, // Gem det unikke Twitter ID
                        Text = text,
                        ImageUrl = mediaUrl,
                        Likes = likes,
                        Retweets = retweets,
                        Replies = replies,
                        PoliticianTwitterId = politicianRecordId, // Brug den fundne interne ID
                        // CreatedAt = createdAt // Kan tilføjes hvis din model har feltet
                    };
                    newTweetEntities.Add(tweetEntity); // Tilføj til listen der skal gemmes
                }
            }

            // 7. Gem alle nye tweet entities til databasen
            if (newTweetEntities.Any()) // Kun hvis der er noget at gemme
            {
                try
                {
                    _dbContext.Tweets.AddRange(newTweetEntities); // Tilføj alle nye på én gang
                    await _dbContext.SaveChangesAsync();          // Gem ændringer til DB
                }
                catch (DbUpdateException ex)
                {
                    // Fejl under skrivning til DB
                    Console.WriteLine($"Error: Failed saving new tweets to database for Politician DB ID {politicianRecordId}: {ex.Message}");
                    // Returner tom liste, da gemningen fejlede
                    return new List<TweetDto>();
                }
            }

            // 8. Returner listen af DTO'er for de nye tweets der blev behandlet/gemt
            return tweetDtos;
        }

        // Privat hjælpefunktion til at hente OpenGraph billede
        private async Task<string?> GetOpenGraphImageAsync(string url)
        {
             try
             {
                 var request = new HttpRequestMessage(HttpMethod.Get, url);
                 // Brug en beskrivende User-Agent
                 request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; PoliticianTweetFetcher/1.0; +http://yourdomain.com/botinfo)");

                 var response = await _httpClient.SendAsync(request);
                 if (!response.IsSuccessStatusCode) return null; // Returner null hvis siden ikke kan hentes

                 var html = await response.Content.ReadAsStringAsync();
                 var doc = new HtmlAgilityPack.HtmlDocument();
                 doc.LoadHtml(html);
                 // Find specifikt 'og:image' meta tag
                 var metaTag = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                 // Returner 'content' attributten hvis tag'et findes
                 return metaTag?.GetAttributeValue("content", null);
             }
             catch (Exception ex) // Undgå at fange alt, men for simpelhedens skyld her
             {
                  // Fejl under scraping (netværk, parsing, timeout etc.)
                  // Console.WriteLine($"Warning: Failed to get OpenGraph image for URL {url}: {ex.Message}"); // Kan aktiveres for debug
                  return null; // Returner null ved fejl
             }
        }

        // Den oprindelige GetUserTweets metode (uden DB interaktion eller dublet-tjek)
        // kan beholdes hvis den bruges andre steder, ellers kan den fjernes.
        public async Task<List<string>> GetUserTweets(string userId, int count = 5)
        {
            string url = $"https://api.twitter.com/2/users/{userId}/tweets" +
                         $"?max_results={count}" +
                         $"&expansions=attachments.media_keys" +
                         $"&media.fields=preview_image_url,url,type" +
                         $"&tweet.fields=entities";

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<string> { $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}" };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonResponse);

            var tweets = json["data"];
            var media = json["includes"]?["media"];
            List<string> tweetTexts = new();

            if (tweets != null)
            {
                foreach (var tweet in tweets)
                {
                    string text = tweet["text"]?.ToString() ?? "";
                    string mediaUrl = "";
                    var mediaKeys = tweet["attachments"]?["media_keys"];
                    if (mediaKeys != null && media != null)
                    {
                        foreach (var key in mediaKeys)
                        {
                            var matchedMedia = media.FirstOrDefault(m => m["media_key"]?.ToString() == key?.ToString());
                            if (matchedMedia != null)
                            {
                                mediaUrl = matchedMedia["url"]?.ToString() ?? matchedMedia["preview_image_url"]?.ToString() ?? "";
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(mediaUrl))
                        tweetTexts.Add($"{text}\n[Billede] {mediaUrl}");
                    else
                        tweetTexts.Add(text);
                }
            }
            return tweetTexts;
        }
    }
}