using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Repositories.Feed
{
    public class FeedRepository : IFeedRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<FeedRepository> _logger;

        public FeedRepository(DataContext context, ILogger<FeedRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<PoliticianInfoDto>> GetUserSubscriptionsAsync(int userId)
        {
            try
            {
                var subscriptions = await _context
                    .Subscriptions.Where(s => s.UserId == userId)
                    .Include(s => s.Politician)
                    .Select(s => new PoliticianInfoDto
                    {
                        Id = s.Politician.Id,
                        Name = s.Politician.Name,
                    })
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                
                return subscriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving subscriptions for user {userId}");
                throw;
            }
        }

        public async Task<List<int>> GetUserSubscribedPoliticianIdsAsync(int userId)
        {
            return await _context
                .Subscriptions.Where(s => s.UserId == userId)
                .Select(s => s.PoliticianTwitterId)
                .ToListAsync();
        }

        public async Task<bool> IsUserSubscribedToPoliticianAsync(int userId, int politicianId)
        {
            return await _context.Subscriptions.AnyAsync(s =>
                s.UserId == userId && s.PoliticianTwitterId == politicianId
            );
        }

        public async Task<List<Tweet>> GetTweetsByPoliticianIdAsync(int politicianId)
        {
            return await _context
                .Tweets.Where(t => t.PoliticianTwitterId == politicianId)
                .OrderByDescending(t => t.CreatedAt)
                .Include(t => t.Politician)
                .ToListAsync();
        }

        public async Task<List<Tweet>> GetTopTweetsByPoliticianIdsAsync(List<int> politicianIds, int topCount)
        {
            var allTweets = new List<Tweet>();
            
            foreach (var polDbId in politicianIds)
            {
                var politicianTopTweets = await _context
                    .Tweets.Where(t => t.PoliticianTwitterId == polDbId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(topCount)
                    .Include(t => t.Politician)
                    .ToListAsync();
                    
                allTweets.AddRange(politicianTopTweets);
            }
            
            return allTweets.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public async Task<Dictionary<int, UserVote>> GetUserVotesForPollsAsync(int userId, List<int> pollIds)
        {
            return await _context
                .UserVotes.Where(uv =>
                    uv.UserId == userId && pollIds.Contains(uv.PollId)
                )
                .ToDictionaryAsync(uv => uv.PollId, uv => uv);
        }

        public async Task<List<Poll>> GetLatestPollsByPoliticianIdsAsync(List<int> politicianIds, int count)
        {
            var allPolls = new List<Poll>();
            
            foreach (var polDbId in politicianIds)
            {
                var politicianLatestPolls = await _context
                    .Polls.Where(p => p.PoliticianTwitterId == polDbId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .Include(p => p.Politician)
                    .Include(p => p.Options)
                    .ToListAsync();
                    
                allPolls.AddRange(politicianLatestPolls);
            }
            
            return allPolls;
        }
    }
}