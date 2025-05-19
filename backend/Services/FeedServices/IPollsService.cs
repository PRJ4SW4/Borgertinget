using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;

namespace backend.Services.Polls
{
    public interface IPollsService
    {
        Task<PollDetailsDto> CreatePollAsync(CreatePollDto createPollDto);
        Task<List<PollSummaryDto>> GetAllPollsAsync();
        Task<PollDetailsDto?> GetPollByIdAsync(int id, int userId);
        Task<bool> ValidatePoll(CreatePollDto pollDto);
        Task<bool> ValidateUpdatePoll(UpdatePollDto updateDto);
        Task<Poll> GetPollAsync(int id);
        Task<bool> UpdatePollAsync(int id, UpdatePollDto updateDto);
        Task<(bool success, List<PollOptionDto> updatedOptions)> VoteAsync(
            int pollId,
            int userId,
            int optionId
        );
        Task<bool> DeletePollAsync(int id);
        Task<PoliticianTwitterId> GetPolitician(int politicianId);
    }
}
