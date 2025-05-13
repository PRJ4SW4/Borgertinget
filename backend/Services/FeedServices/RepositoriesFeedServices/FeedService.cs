using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Repository for feed-related data access
public class FeedRepository : IFeedRepository
{
    private readonly DataContext _context;

    public FeedRepository(DataContext context)
    {
        _context = context;
    }

    // Henter alle politikere som brugeren følger (abonnementer)
    public async Task<List<PoliticianInfoDto>> GetUserSubscriptionsAsync(int userId)
    {
        // Finder alle subscriptions for brugeren og returnerer politikernes info som DTO
        return await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Include(s => s.Politician)
            .Select(s => new PoliticianInfoDto { Id = s.Politician.Id, Name = s.Politician.Name })
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    // Henter ID'er på alle politikere som brugeren følger
    public async Task<List<int>> GetUserPoliticianIdsAsync(int userId)
    {
        // Returnerer kun ID'erne (bruges til at filtrere tweets/polls)
        return await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.PoliticianTwitterId)
            .ToListAsync();
    }

    // Tjekker om brugeren følger en bestemt politiker
    public async Task<bool> IsUserSubscribedToPoliticianAsync(int userId, int politicianId)
    {
        // Returnerer true hvis abonnement findes, ellers false
        return await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.PoliticianTwitterId == politicianId);
    }

    // Henter alle tweets for en bestemt politiker (evt. begrænset antal)
    public async Task<List<Tweet>> GetTweetsForPoliticianAsync(int politicianId, int take = 0)
    {
        // Finder tweets for politikeren, sorteret nyeste først
        var query = _context.Tweets
            .Include(t => t.Politician)
            .Where(t => t.PoliticianTwitterId == politicianId)
            .OrderByDescending(t => t.CreatedAt);

        // Hvis take > 0, begræns antal tweets
        if (take > 0)
            return await query.Take(take).ToListAsync();

        return await query.ToListAsync();
    }

    // Henter de seneste tweets for flere politikere (fx 5 pr. politiker)
    public async Task<List<Tweet>> GetTweetsForPoliticiansAsync(IEnumerable<int> politicianIds, int takePerPolitician = 5)
    {
        var allTweets = new List<Tweet>();
        // For hver politiker hentes de seneste tweets
        foreach (var polId in politicianIds)
        {
            var tweets = await GetTweetsForPoliticianAsync(polId, takePerPolitician);
            allTweets.AddRange(tweets);
        }
        // Samlet liste sorteres nyeste først
        return allTweets.OrderByDescending(t => t.CreatedAt).ToList();
    }

    // Henter de seneste polls for flere politikere (fx 2 pr. politiker)
    public async Task<List<Poll>> GetLatestPollsForPoliticiansAsync(IEnumerable<int> politicianIds, int takePerPolitician = 2)
    {
        var allPolls = new List<Poll>();
        // For hver politiker hentes de seneste polls inkl. options og politiker-info
        foreach (var polId in politicianIds)
        {
            var polls = await _context.Polls
                .Where(p => p.PoliticianTwitterId == polId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(takePerPolitician)
                .Include(p => p.Politician)
                .Include(p => p.Options)
                .ToListAsync();
            allPolls.AddRange(polls);
        }
        return allPolls;
    }

    // Henter brugerens stemmer på en række polls (bruges til at vise om brugeren har stemt)
    public async Task<Dictionary<int, UserVote>> GetUserVotesForPollsAsync(int userId, IEnumerable<int> pollIds)
    {
        // Returnerer et dictionary hvor key er pollId og value er brugerens stemme
        return await _context.UserVotes
            .Where(uv => uv.UserId == userId && pollIds.Contains(uv.PollId))
            .ToDictionaryAsync(uv => uv.PollId, uv => uv);
    }
}