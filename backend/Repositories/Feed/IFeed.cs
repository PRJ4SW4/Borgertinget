using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;

namespace backend.Repositories.Feed
{
    public interface IFeedRepository
    {
        Task<List<PoliticianInfoDto>> GetUserSubscriptionsAsync(int userId);
        Task<List<int>> GetUserSubscribedPoliticianIdsAsync(int userId);
        Task<bool> IsUserSubscribedToPoliticianAsync(int userId, int politicianId);
        Task<List<Tweet>> GetTweetsByPoliticianIdAsync(int politicianId);
        Task<List<Tweet>> GetTopTweetsByPoliticianIdsAsync(List<int> politicianIds, int topCount);
        Task<Dictionary<int, UserVote>> GetUserVotesForPollsAsync(int userId, List<int> pollIds);
        Task<List<Poll>> GetLatestPollsByPoliticianIdsAsync(List<int> politicianIds, int count);
    }
}
