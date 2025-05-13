using backend.DTOs;
using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFeedRepository
{
    Task<List<PoliticianInfoDto>> GetUserSubscriptionsAsync(int userId);
    Task<List<int>> GetUserPoliticianIdsAsync(int userId);
    Task<bool> IsUserSubscribedToPoliticianAsync(int userId, int politicianId);
    Task<List<Tweet>> GetTweetsForPoliticianAsync(int politicianId, int take = 0);
    Task<List<Tweet>> GetTweetsForPoliticiansAsync(IEnumerable<int> politicianIds, int takePerPolitician = 5);
    Task<List<Poll>> GetLatestPollsForPoliticiansAsync(IEnumerable<int> politicianIds, int takePerPolitician = 2);
    Task<Dictionary<int, UserVote>> GetUserVotesForPollsAsync(int userId, IEnumerable<int> pollIds);
}