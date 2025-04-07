// Husk relevante using statements øverst i filen
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
// using System.Security.Claims; // Til rigtig bruger ID senere
using System.Threading.Tasks;

namespace backend.Controllers // Sørg for at namespace passer
{
    [ApiController]
    [Route("api")] // Base route, endpoint bliver /api/feed
    public class FeedController : ControllerBase
    {
        private readonly DataContext _context;

        // Inject DataContext. Overvej en FeedService senere for bedre struktur.
        public FeedController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("feed")]
public async Task<ActionResult<List<TweetDto>>> GetMyFeed(/* CancellationToken cancellationToken */)
{
    int currentUserId = 1; // Stadig hardcoded til test

    try
    {
        // 1. Find fulgte politikere (som før)
        var subscribedPoliticianIds = await _context.Subscriptions
            .Where(s => s.UserId == currentUserId)
            .Select(s => s.PoliticianTwitterId)
            .ToListAsync(/* cancellationToken */);

        if (!subscribedPoliticianIds.Any())
        {
            return Ok(new List<TweetDto>());
        }

        // --- Omskrivning starter her ---

        // 2. Opret en liste til at samle resultaterne
        var allFeedTweets = new List<Tweet>();

        // 3. Loop igennem hver fulgt politiker
        foreach (var polId in subscribedPoliticianIds)
        {
            // 4. Hent top 5 for DENNE politiker (simpel query)
            var politicianTop5Tweets = await _context.Tweets
                .Where(t => t.PoliticianTwitterId == polId)
                .OrderByDescending(t => t.CreatedAt) // Kræver CreatedAt!
                .Take(5)
                .Include(t => t.Politician) // Stadig Include for Name/Handle
                .ToListAsync(/* cancellationToken */);

            // 5. Tilføj dem til den samlede liste
            allFeedTweets.AddRange(politicianTop5Tweets);
        }

        // 6. Sortér den samlede liste i hukommelsen (efter alle DB kald)
        var sortedFeedTweets = allFeedTweets.OrderByDescending(t => t.CreatedAt).ToList();

        // 7. Map til DTO'er (som før)
        var feedDtos = sortedFeedTweets
            .Select(t => new TweetDto
            {
                TwitterTweetId = t.TwitterTweetId,
                Text = t.Text,
                ImageUrl = t.ImageUrl,
                Likes = t.Likes,
                Retweets = t.Retweets,
                Replies = t.Replies,
                CreatedAt = t.CreatedAt,
                AuthorName = t.Politician?.Name ?? "Ukendt Afsender",
                AuthorHandle = t.Politician?.TwitterHandle ?? "ukendt"
            })
            .ToList();

        // --- Omskrivning slutter her ---

        return Ok(feedDtos);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fejl under hentning af feed for bruger {currentUserId}: {ex}");
        return StatusCode(500, "Der opstod en intern fejl under hentning af dit feed.");
    }
}}}