using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using backend.Data;
using backend.DTOs;

public class TwitterService
{
    private readonly HttpClient _httpClient;
    private readonly string _bearerToken;

    
    private readonly  DataContext _dbContext;  


    public TwitterService(HttpClient httpClient, IConfiguration config, DataContext dbContext )
    {
        _httpClient = httpClient;
        _bearerToken = config["TwitterApi:BearerToken"];
        _dbContext = dbContext;
    }


public async Task<List<string>> GetUserTweets(string userId, int count = 5) // kan godt være vi skal ændre den til at tahe 10 tweets fra hvert ide
{
    // 1. Bygger URL'en til Twitter API, inkl. funktioner tager userId, som er den bruger vi vil hente tweets fra og count, som er hvor mange tweets vi vil hente
    // herunder definere vi så hvad vi vil have med i vores response, som er medier og link
    string url = $"https://api.twitter.com/2/users/{userId}/tweets" +
                 $"?max_results={count}" +
                 $"&expansions=attachments.media_keys" +         // henter medier som billeder/videoer
                 $"&media.fields=preview_image_url,url,type" +   // vi vil bruge billed-url'en
                 $"&tweet.fields=entities";                      // giver os adgang til link (expanded_url)


        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken); // her giver vi vores bearer token tik twitter api, så den ved hvem vi er og hvad vi har adgang til

        var response = await _httpClient.GetAsync(url); // her sender vi så vores request til twitter api

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

    public async Task<List<TweetDto>> GetStructuredTweets(string userId, int count = 5)
    {
        string url = $"https://api.twitter.com/2/users/{userId}/tweets" +
                     $"?max_results={count}" +
                     $"&expansions=attachments.media_keys" +
                     $"&media.fields=preview_image_url,url,type" +
                     "&tweet.fields=entities,public_metrics";
                     // Dette er er de ting vi henter fra public_metrics for at få likes og retweets.


        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return new List<TweetDto>(); // tom liste ved fejl
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(jsonResponse);

        var tweets = json["data"];
        var media = json["includes"]?["media"];
        var tweetDtos = new List<TweetDto>();

        if (tweets != null)
        {
           foreach (var tweet in tweets)
{
    string text = tweet["text"]?.ToString() ?? "";
    // Fjern t.co-links fra teksten
    text = System.Text.RegularExpressions.Regex.Replace(text, @"https:\/\/t\.co\/\S+", "").Trim();

    string mediaUrl = "";

    //  1. Tjek efter billede fra Twitter-medier
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

    //  2. Hvis intet billede fra Twitter, tjek efter link (og hent OpenGraph-billede)
    if (string.IsNullOrEmpty(mediaUrl))
    {
        var link = tweet["entities"]?["urls"]?.FirstOrDefault()?["expanded_url"]?.ToString();

        if (!string.IsNullOrEmpty(link))
        {
            var ogImage = await GetOpenGraphImageAsync(link);
            if (!string.IsNullOrEmpty(ogImage))
                mediaUrl = ogImage;
        }
    }

    //  3. Hent antal likes, retweets og replies fra public_metrics
    var metrics = tweet["public_metrics"];
    int likes = metrics?["like_count"]?.ToObject<int>() ?? 0;
    int retweets = metrics?["retweet_count"]?.ToObject<int>() ?? 0;
    int replies = metrics?["reply_count"]?.ToObject<int>() ?? 0;

    var tweetDto = new TweetDto
                {
                    Text = text,
                    ImageUrl = mediaUrl,
                    Likes = likes,
                    Retweets = retweets,
                    Replies = replies
                };
                tweetDtos.Add(tweetDto);

                // gem tweet i databasen
                var tweetEntity = new Tweet
                {
                    Text = text,
                    ImageUrl = mediaUrl,
                    Likes = likes,
                    Retweets = retweets,
                    Replies = replies,
                    UserId = userId,  
                    
                };
                _dbContext.Tweets.Add(tweetEntity); 
            }

            await _dbContext.SaveChangesAsync();  
        }

        return tweetDtos;
    }

    

    private async Task<string?> GetOpenGraphImageAsync(string url)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var metaTags = doc.DocumentNode.SelectNodes("//meta");

            var ogImage = metaTags?
                .FirstOrDefault(m => m.GetAttributeValue("property", "") == "og:image");

            return ogImage?.GetAttributeValue("content", null);
        }
        catch
        {
            return null;
        }
    }
}

