using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;

namespace backend.Repositories.Polls
{
    public interface IPollsRepository
    {
        Task<Poll> CreatePollAsync(Poll poll);
        Task<List<PollSummaryDto>> GetAllPollsAsync();
        Task<Poll?> GetPollByIdAsync(int id);
        Task<UserVote?> GetUserVoteAsync(int pollId, int userId);
        Task<PoliticianTwitterId?> GetPoliticianByIdAsync(int politicianId);
        Task<bool> UpdatePollAsync(Poll poll);
        Task<bool> VoteAsync(int pollId, int userId, int optionId);
        Task<bool> DeletePollAsync(int id);
    }
}
