using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GetMySubscriptions: Henter brugerens abonnementer på politikere

        // først findes brugeren user id fra jwt fra token
        // Returnerer en liste af politikere som brugeren følger
        [Authorize]
        [HttpGet("subscriptions")]
        public async Task<ActionResult<List<PoliticianInfoDto>>> GetMySubscriptions()
        {
            var userIdString = User.FindFirstValue("userId");
            if (
                string.IsNullOrEmpty(userIdString)
                || !int.TryParse(userIdString, out int currentUserId)
            )
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            try
            { // så laves der et db query til subscriptions, som henter alle subscriptions for den bruger, og henter derefter politikernes data vhj. dto.
                var subscriptions = await _context
                    .Subscriptions.Where(s => s.UserId == currentUserId)
                    .Include(s => s.Politician)
                    .Select(s => new PoliticianInfoDto
                    {
                        Id = s.Politician.Id,
                        Name = s.Politician.Name,
                    })
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Fejl under hentning af subscriptions for bruger {currentUserId}: {ex}"
                );
                return StatusCode(500, "Intern fejl ved hentning af abonnementer.");
            }
        }

        //  GetMyFeed dette er så selve "cobtroller" til at hente tweets og polls
        [Authorize]
        [HttpGet("feed")]
        // Returnerer den PaginatedFeedResult DTO, som nu har Tweets og LatestPolls
        public async Task<ActionResult<PaginatedFeedResult>> GetMyFeed(
            // Paginering: page og pageSize standardværdier, vi starter på page 1 og 5 tweets pr. side
            [FromQuery]
                int page = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] int? politicianId = null
        )
        {
            // først findes brugeren user id fra jwt fra token, samme stil, som før i "GetMySubscriptions" endpointet
            var userIdString = User.FindFirstValue("userId");
            if (
                string.IsNullOrEmpty(userIdString)
                || !int.TryParse(userIdString, out int currentUserId)
            )
            {
                return Unauthorized("Kunne ikke identificere brugeren korrekt fra token.");
            }

            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = 5;
            if (pageSize > 50)
                pageSize = 50;

            try
            {
                // Filter-logik for feed:
                // 1. Hvis filter er sat, tjekkes der om brugeren følger den valgte politiker
                // 2. Hvis ikke filter: Henter ID'er på alle politikere brugeren følger
                // 3. Resultatet bruges til at filtrere tweets i den efterfølgende kode
                // 4. Returnerer tom liste hvis brugeren ikke følger nogen eller den valgte politiker

                List<int> relevantPoliticianDbIds;
                bool isFiltered = politicianId.HasValue;

                if (isFiltered) // Filter er sat
                {
                    // Tjek om brugeren følger den filtrerede politiker
                    bool isSubscribed = await _context.Subscriptions.AnyAsync(s =>
                        s.UserId == currentUserId && s.PoliticianTwitterId == politicianId!.Value
                    );
                    if (!isSubscribed)
                        return Ok(new PaginatedFeedResult()); // Tomt hvis der ikke følges nogen :-(
                    relevantPoliticianDbIds = new List<int> { politicianId!.Value }; // måske slet !her
                }
                else // Intet filter ("Alle Tweets" view)
                {
                    relevantPoliticianDbIds = await _context
                        .Subscriptions.Where(s => s.UserId == currentUserId)
                        .Select(s => s.PoliticianTwitterId)
                        .ToListAsync();
                }

                if (!relevantPoliticianDbIds.Any())
                {
                    return Ok(new PaginatedFeedResult()); // Tom hvis ingen følges, :-(
                }

                // her er det så selve logikken til at hente først ##tweets## og derefter  kommer ##polls## længere nede.
                List<Tweet> tweetsToPaginate;
                if (isFiltered) // Filtreret: Hent alle tweets for den ene politiker
                {
                    tweetsToPaginate = await _context
                        .Tweets.Where(t => t.PoliticianTwitterId == politicianId!.Value)
                        .OrderByDescending(t => t.CreatedAt)
                        .Include(t => t.Politician)
                        .ToListAsync();
                }
                else // Ufiltreret: Hent top 5 tweets per politiker og aggreger
                {
                    var allPotentialFeedTweets = new List<Tweet>();
                    foreach (var polDbId in relevantPoliticianDbIds)
                    {
                        var politicianTop5Tweets = await _context
                            .Tweets.Where(t => t.PoliticianTwitterId == polDbId)
                            .OrderByDescending(t => t.CreatedAt)
                            .Take(5)
                            .Include(t => t.Politician)
                            .ToListAsync();
                        allPotentialFeedTweets.AddRange(politicianTop5Tweets);
                    }
                    tweetsToPaginate = allPotentialFeedTweets
                        .OrderByDescending(t => t.CreatedAt)
                        .ToList();
                }

                // logik af paginering af Tweets pr side
                // Her opdeles tweets i "sider" baseret på page og pageSize parametre:
                // 1. For side 1: Skip (1-1)*5 = 0 tweets (viser tweets 1-5)
                // 2. For side 2: Skip (2-1)*5 = 5 tweets (viser tweets 6-10)
                // 3. For side 3: Skip (2-1)*5 = 10 tweets (viser tweets 11-15)
                // Beregner også om der er flere tweets at hente (hasMoreTweets)


                int totalTweets = tweetsToPaginate.Count;
                int skipAmountTweets = (page - 1) * pageSize;
                var pagedTweets = tweetsToPaginate.Skip(skipAmountTweets).Take(pageSize).ToList();
                bool hasMoreTweets = skipAmountTweets + pagedTweets.Count < totalTweets;

                // Map de paginerede tweets til, da det  TweetDTOs, som er hvad vi gerne vil returnere til frontend
                var feedTweetDtos = pagedTweets
                    .Select(t => new TweetDto
                    {
                        TwitterTweetId = t.TwitterTweetId,
                        Text = t.Text,
                        ImageUrl = t.ImageUrl,
                        Likes = t.Likes,
                        Retweets = t.Retweets,
                        Replies = t.Replies,
                        CreatedAt = t.CreatedAt,
                        AuthorName = t.Politician?.Name ?? "Ukendt",
                        AuthorHandle = t.Politician?.TwitterHandle ?? "ukendt",
                    })
                    .ToList();

                // Her er logikken til at hente ##polls##, jeg har valgt at, der skal hentes 2 polls pr politiker, og så vil den returnere dem i en liste.
                // Hvis der er filtreret, vil den ikke hente polls, og derfor vil latestPollDtos være en tom liste.
                List<PollDetailsDto> latestPollDtos = new List<PollDetailsDto>(); // Start med tom liste

                if (!isFiltered) // Kør kun denne logik, hvis vi ser "Alle Tweets"
                {
                    var allLatestPolls = new List<Poll>();
                    // Loop gennem ALLE fulgte politikere
                    foreach (var polDbId in relevantPoliticianDbIds)
                    {
                        var politicianLatest2Polls = await _context
                            .Polls.Where(p => p.PoliticianTwitterId == polDbId.ToString())
                            .OrderByDescending(p => p.CreatedAt)
                            .Take(2)
                            .Include(p => p.Politician)
                            .Include(p => p.Options)
                            .ToListAsync();
                        allLatestPolls.AddRange(politicianLatest2Polls);
                    }

                    // Hent brugerens stemmer for de indsamlede polls
                    var pollIdsToCheck = allLatestPolls.Select(p => p.Id).Distinct().ToList();
                    var userVotesForLatestPolls = await _context
                        .UserVotes.Where(uv =>
                            uv.UserId == currentUserId && pollIdsToCheck.Contains(uv.PollId)
                        )
                        .ToDictionaryAsync(uv => uv.PollId, uv => uv);

                    // Map og sorter de indsamlede polls globalt
                    latestPollDtos = allLatestPolls
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p =>
                            MapPollToDetailsDto(
                                p,
                                p.Politician,
                                userVotesForLatestPolls.ContainsKey(p.Id)
                                    ? userVotesForLatestPolls[p.Id]
                                    : null
                            )
                        )
                        .ToList();
                }
                // Hvis isFiltered er true, forbliver latestPollDtos en tom liste.

                // i et fælles feed, men stadig 2 lister, så vi kan vise dem i et fælles feed.
                var result = new PaginatedFeedResult
                {
                    Tweets = feedTweetDtos, // Paginerede tweets
                    HasMore = hasMoreTweets, // Paginering baseret på tweets
                    LatestPolls = latestPollDtos, // Seneste 2 polls per politiker (eller tom liste hvis filtreret)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Fejl under hentning af feed for bruger {currentUserId} (Filter: {politicianId}): {ex}"
                );
                return StatusCode(500, "Der opstod en intern fejl under hentning af dit feed.");
            }
        }

        //  Privat Hjælpemetode til Poll Mapping med dto
        // Mapper en Poll til en PollDetailsDto, som indeholder alle relevante oplysninger om poll'en
        private PollDetailsDto MapPollToDetailsDto(
            Poll poll,
            PoliticianTwitterId politician,
            UserVote? userVote
        )
        {
            int totalVotes = poll.Options?.Sum(o => o.Votes) ?? 0;
            return new PollDetailsDto
            {
                Id = poll.Id,
                Question = poll.Question,
                CreatedAt = poll.CreatedAt,
                EndedAt = poll.EndedAt,
                PoliticianId = politician.Id,
                PoliticianName = politician.Name,
                PoliticianHandle = politician.TwitterHandle,
                Options =
                    poll.Options?.Select(o => new PollOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            Votes = o.Votes,
                        })
                        .OrderBy(o => o.Id)
                        .ToList() ?? new List<PollOptionDto>(),
                CurrentUserVoteOptionId = userVote?.ChosenOptionId,
                TotalVotes = totalVotes,
            };
        }
    }
}
