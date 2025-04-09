// Husk relevante using statements øverst i filen
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

        [Authorize]
        [HttpGet("feed")]
       
        public async Task<ActionResult<PaginatedFeedResult>> GetMyFeed(
            [FromQuery] int page = 1,        // Default til side 1
            [FromQuery] int pageSize = 5)    // Default til 5 tweets pr. side
            /* CancellationToken cancellationToken */
        {
            // --- Hent Bruger ID ---
            var userIdString = User.FindFirstValue("userId"); // Bruger "userId" claim
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Kunne ikke identificere brugeren korrekt fra token.");
            }

            // Sørg for at page og pageSize har fornuftige værdier
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50; // Sæt evt. en øvre grænse

            try
            {
                // --- Hent data fra DB (N+1 loop metode) ---

                // Find fulgte politikere
                var subscribedPoliticianIds = await _context.Subscriptions
                    .Where(s => s.UserId == currentUserId)
                    .Select(s => s.PoliticianTwitterId)
                    .ToListAsync(/* cancellationToken */);

                if (!subscribedPoliticianIds.Any())
                {
                    // Returner tomt resultat hvis ingen abonnementer
                    return Ok(new PaginatedFeedResult { Tweets = new List<TweetDto>(), HasMore = false });
                }

                // Saml top 5 tweets fra HVER politiker
                var allPotentialFeedTweets = new List<Tweet>();
                foreach (var polId in subscribedPoliticianIds)
                {
                    var politicianTop5Tweets = await _context.Tweets
                        .Where(t => t.PoliticianTwitterId == polId)
                        .OrderByDescending(t => t.CreatedAt) // Kræver CreatedAt!
                        .Take(5)
                        .Include(t => t.Politician)
                        .ToListAsync(/* cancellationToken */);
                    allPotentialFeedTweets.AddRange(politicianTop5Tweets);
                }

                // --- Pagination Logik ---

                // 1. Sortér ALLE de potentielle tweets (nyeste først)
                var sortedTweets = allPotentialFeedTweets.OrderByDescending(t => t.CreatedAt).ToList();

                // 2. Beregn totalt antal og hvor mange der skal springes over
                int totalTweets = sortedTweets.Count;
                int skipAmount = (page - 1) * pageSize;

                // 3. Udtag den aktuelle side af tweets
                var pagedTweets = sortedTweets.Skip(skipAmount).Take(pageSize).ToList();

                // 4. Beregn om der er flere sider
                bool hasMore = skipAmount + pagedTweets.Count < totalTweets;

                // 5. Map den aktuelle sides tweets til DTO'er
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

                // 6. Opret og returner det paginerede resultat
                var result = new PaginatedFeedResult
                {
                    Tweets = feedDtos,
                    HasMore = hasMore
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af pagineret feed for bruger {currentUserId}: {ex}");
                return StatusCode(500, "Der opstod en intern fejl under hentning af dit feed.");
            }
        }
    }
}