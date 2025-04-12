// backend.Controllers/FeedController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class FeedController : ControllerBase
    {
        private readonly DataContext _context;

        public FeedController(DataContext context)
        {
            _context = context;
        }

        // --- NYT ENDPOINT TIL AT HENTE ABONNEMENTER ---
        [Authorize]
        [HttpGet("subscriptions")] // Lytter på GET /api/subscriptions
        public async Task<ActionResult<List<PoliticianInfoDto>>> GetMySubscriptions()
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            try
            {
                // Hent abonnementer, inkluder Politiker-data
                var subscriptions = await _context.Subscriptions
                    .Where(s => s.UserId == currentUserId)
                    .Include(s => s.Politician) // Inkluder relateret PoliticianTwitterId objekt
                    .Select(s => new PoliticianInfoDto
                    {
                        // Brug Id og Name fra den inkluderede Politician (PoliticianTwitterId model)
                        Id = s.Politician.Id,
                        Name = s.Politician.Name
                    })
                    .OrderBy(p => p.Name) // Sorter alfabetisk
                    .ToListAsync();

                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af subscriptions for bruger {currentUserId}: {ex}");
                return StatusCode(500, "Intern fejl ved hentning af abonnementer.");
            }
        }


        // --- OPDATERET GetMyFeed MED FILTER ---
        [Authorize]
        [HttpGet("feed")]
        public async Task<ActionResult<PaginatedFeedResult>> GetMyFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5, // Husk at matche frontend page size hvis relevant
            [FromQuery] int? politicianId = null) // Valgfri filter parameter (bruger politikerens DB ID)
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Kunne ikke identificere brugeren korrekt fra token.");
            }

            // Sørg for at page og pageSize har fornuftige værdier
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;

            try
            {
                List<Tweet> tweetsToPaginate; // Liste vi vil paginere til sidst

                // ----- Logik for filtreret visning -----
                if (politicianId.HasValue)
                {
                    // Tjek evt. først om brugeren følger denne politiker for ekstra sikkerhed
                    // bool isSubscribed = await _context.Subscriptions.AnyAsync(s => s.UserId == currentUserId && s.PoliticianId == politicianId.Value); // Kræver PoliticianId i Subscription
                    // if (!isSubscribed) return Forbid("Du følger ikke denne politiker.");

                    // Hent tweets KUN fra den specifikke politiker (bruger FK: PoliticianTwitterId i Tweet modellen)
                    tweetsToPaginate = await _context.Tweets
                        .Where(t => t.PoliticianTwitterId == politicianId.Value) // Filter på politikerens DB ID
                        .OrderByDescending(t => t.CreatedAt)
                        .Include(t => t.Politician) // Stadig nødvendigt for AuthorName/Handle
                        .ToListAsync();
                    // Vi henter ALLE tweets for den ene politiker og paginerer i hukommelsen
                }
                // ----- Logik for ufiltreret "Alle Tweets" visning (Oprindelig logik) -----
                else
                {
                    // Find fulgte politikere (DB IDs)
                    // *VIGTIGT:* Vi skal bruge politikerens DB ID her, ikke TwitterUserId, for at matche politicianId parameteren
                    var subscribedPoliticianDbIds = await _context.Subscriptions
                        .Where(s => s.UserId == currentUserId)
                        .Select(s => s.PoliticianTwitterId) // Dette henter FK'en (politikerens DB ID)
                        .ToListAsync();

                    if (!subscribedPoliticianDbIds.Any())
                    {
                        return Ok(new PaginatedFeedResult { Tweets = new List<TweetDto>(), HasMore = false });
                    }

                    // Saml top 5 tweets fra HVER politiker (N+1 Loop)
                    var allPotentialFeedTweets = new List<Tweet>();
                    foreach (var polDbId in subscribedPoliticianDbIds)
                    {
                        var politicianTop5Tweets = await _context.Tweets
                            .Where(t => t.PoliticianTwitterId == polDbId) // Filter på politikerens DB ID
                            .OrderByDescending(t => t.CreatedAt)
                            .Take(5)
                            .Include(t => t.Politician)
                            .ToListAsync();
                        allPotentialFeedTweets.AddRange(politicianTop5Tweets);
                    }

                    // Sortér den samlede liste globalt
                    tweetsToPaginate = allPotentialFeedTweets.OrderByDescending(t => t.CreatedAt).ToList();
                }

                // --- FÆLLES Pagination Logik (kører på 'tweetsToPaginate') ---
                int totalTweets = tweetsToPaginate.Count;
                int skipAmount = (page - 1) * pageSize;
                var pagedTweets = tweetsToPaginate.Skip(skipAmount).Take(pageSize).ToList();
                bool hasMore = skipAmount + pagedTweets.Count < totalTweets;

                // --- Map og Returner ---
                var feedDtos = pagedTweets
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

                var result = new PaginatedFeedResult
                {
                    Tweets = feedDtos,
                    HasMore = hasMore
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af pagineret feed for bruger {currentUserId} (Filter: {politicianId}): {ex}");
                return StatusCode(500, "Der opstod en intern fejl under hentning af dit feed.");
            }
        }
    }
}