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

        // --- Endpoint til at hente abonnementer ---
        [Authorize]
        [HttpGet("subscriptions")]
        public async Task<ActionResult<List<PoliticianInfoDto>>> GetMySubscriptions()
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            { return Unauthorized("Kunne ikke identificere brugeren."); }

            try
            {
                var subscriptions = await _context.Subscriptions
                    .Where(s => s.UserId == currentUserId)
                    .Include(s => s.Politician)
                    .Select(s => new PoliticianInfoDto { Id = s.Politician.Id, Name = s.Politician.Name })
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af subscriptions for bruger {currentUserId}: {ex}");
                return StatusCode(500, "Intern fejl ved hentning af abonnementer.");
            }
        }

        // --- GetMyFeed (Henter paginerede tweets OG seneste 2 polls per politiker UDEN filter) ---
        [Authorize]
        [HttpGet("feed")]
        // Returnerer den PaginatedFeedResult DTO, som nu har Tweets og LatestPolls
        public async Task<ActionResult<PaginatedFeedResult>> GetMyFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] int? politicianId = null)
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            { return Unauthorized("Kunne ikke identificere brugeren korrekt fra token."); }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;

            try
            {
                // 1. Find relevante Politiker DB IDs
                List<int> relevantPoliticianDbIds;
                bool isFiltered = politicianId.HasValue; // Tjek om filter er sat

                if (isFiltered) // Filter er sat
                {
                    // Tjek om brugeren følger den filtrerede politiker
                    bool isSubscribed = await _context.Subscriptions.AnyAsync(s => s.UserId == currentUserId && s.PoliticianTwitterId == politicianId.Value);
                    if (!isSubscribed) return Ok(new PaginatedFeedResult()); // Tomt hvis ikke fulgt
                    relevantPoliticianDbIds = new List<int> { politicianId.Value };
                }
                else // Intet filter ("Alle Tweets" view)
                {
                    relevantPoliticianDbIds = await _context.Subscriptions
                        .Where(s => s.UserId == currentUserId)
                        .Select(s => s.PoliticianTwitterId)
                        .ToListAsync();
                }

                if (!relevantPoliticianDbIds.Any())
                {
                    return Ok(new PaginatedFeedResult()); // Tom hvis ingen følges
                }

                // --- Del 1: Hent og Paginér Tweets ---
                List<Tweet> tweetsToPaginate;
                if (isFiltered) // Filtreret: Hent alle tweets for den ene politiker
                {
                    tweetsToPaginate = await _context.Tweets
                        .Where(t => t.PoliticianTwitterId == politicianId.Value)
                        .OrderByDescending(t => t.CreatedAt)
                        .Include(t => t.Politician)
                        .ToListAsync();
                }
                else // Ufiltreret: Hent top 5 tweets per politiker og aggreger
                {
                    var allPotentialFeedTweets = new List<Tweet>();
                    foreach (var polDbId in relevantPoliticianDbIds)
                    {
                        var politicianTop5Tweets = await _context.Tweets
                            .Where(t => t.PoliticianTwitterId == polDbId)
                            .OrderByDescending(t => t.CreatedAt).Take(5)
                            .Include(t => t.Politician).ToListAsync();
                        allPotentialFeedTweets.AddRange(politicianTop5Tweets);
                    }
                    tweetsToPaginate = allPotentialFeedTweets.OrderByDescending(t => t.CreatedAt).ToList();
                }

                // Anvend paginering på den relevante tweet-liste
                int totalTweets = tweetsToPaginate.Count;
                int skipAmountTweets = (page - 1) * pageSize;
                var pagedTweets = tweetsToPaginate.Skip(skipAmountTweets).Take(pageSize).ToList();
                bool hasMoreTweets = skipAmountTweets + pagedTweets.Count < totalTweets;

                // Map de paginerede tweets til DTOs
                var feedTweetDtos = pagedTweets.Select(t => new TweetDto {
                    TwitterTweetId = t.TwitterTweetId, Text = t.Text, ImageUrl = t.ImageUrl, Likes = t.Likes,
                    Retweets = t.Retweets, Replies = t.Replies, CreatedAt = t.CreatedAt,
                    AuthorName = t.Politician?.Name ?? "Ukendt", AuthorHandle = t.Politician?.TwitterHandle ?? "ukendt"
                }).ToList();


                // --- Del 2: Hent de 2 seneste Polls PER POLITIKER (KUN hvis der IKKE er filter) ---
                List<PollDetailsDto> latestPollDtos = new List<PollDetailsDto>(); // Start med tom liste

                if (!isFiltered) // Kør kun denne logik, hvis vi ser "Alle Tweets"
                {
                    var allLatestPolls = new List<Poll>();
                    // Loop gennem ALLE fulgte politikere
                    foreach (var polDbId in relevantPoliticianDbIds)
                    {
                        var politicianLatest2Polls = await _context.Polls
                            .Where(p => p.PoliticianTwitterId == polDbId)
                            .OrderByDescending(p => p.CreatedAt)
                            .Take(2)
                            .Include(p => p.Politician)
                            .Include(p => p.Options)
                            .ToListAsync();
                        allLatestPolls.AddRange(politicianLatest2Polls);
                    }

                    // Hent brugerens stemmer for de indsamlede polls
                    var pollIdsToCheck = allLatestPolls.Select(p => p.Id).Distinct().ToList();
                    var userVotesForLatestPolls = await _context.UserVotes
                       .Where(uv => uv.UserId == currentUserId && pollIdsToCheck.Contains(uv.PollId))
                       .ToDictionaryAsync(uv => uv.PollId, uv => uv);

                    // Map og sorter de indsamlede polls globalt
                    latestPollDtos = allLatestPolls
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => MapPollToDetailsDto(p, p.Politician, userVotesForLatestPolls.ContainsKey(p.Id) ? userVotesForLatestPolls[p.Id] : null))
                        .ToList();
                }
                // Hvis isFiltered er true, forbliver latestPollDtos en tom liste.

                // --- Del 3: Kombiner i Resultatet ---
                var result = new PaginatedFeedResult
                {
                    Tweets = feedTweetDtos,          // Paginerede tweets
                    HasMore = hasMoreTweets,         // Paginering baseret på tweets
                    LatestPolls = latestPollDtos     // Seneste 2 polls per politiker (eller tom liste hvis filtreret)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af feed for bruger {currentUserId} (Filter: {politicianId}): {ex}");
                return StatusCode(500, "Der opstod en intern fejl under hentning af dit feed.");
            }
        }

        // --- Privat Hjælpemetode til Poll Mapping (som før) ---
        private PollDetailsDto MapPollToDetailsDto(Poll poll, PoliticianTwitterId politician, UserVote? userVote)
        {
             int totalVotes = poll.Options?.Sum(o => o.Votes) ?? 0;
             return new PollDetailsDto {
                 Id = poll.Id, Question = poll.Question, CreatedAt = poll.CreatedAt, EndedAt = poll.EndedAt,
                 PoliticianId = politician.Id, PoliticianName = politician.Name, PoliticianHandle = politician.TwitterHandle,
                 Options = poll.Options?.Select(o => new PollOptionDto { Id = o.Id, OptionText = o.OptionText, Votes = o.Votes })
                                    .OrderBy(o => o.Id).ToList() ?? new List<PollOptionDto>(),
                 CurrentUserVoteOptionId = userVote?.ChosenOptionId, TotalVotes = totalVotes
             };
        }
     

    }

} // Slut på namespace