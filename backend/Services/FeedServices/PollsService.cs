using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;
using backend.Repositories.Polls;
using Microsoft.Extensions.Logging;

namespace backend.Services.Polls
{
    public class PollsService : IPollsService
    {
        private readonly IPollsRepository _repository;
        private readonly ILogger<PollsService> _logger;

        public PollsService(IPollsRepository repository, ILogger<PollsService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PollDetailsDto> CreatePollAsync(CreatePollDto createPollDto)
        {
            var newPoll = new Poll
            {
                Question = createPollDto.Question,
                PoliticianTwitterId = createPollDto.PoliticianTwitterId,
                CreatedAt = DateTime.UtcNow,
                EndedAt = createPollDto.EndedAt?.ToUniversalTime(),
                Options = new List<PollOption>()
            };

            foreach (var optionText in createPollDto.Options)
            {
                newPoll.Options.Add(new PollOption { OptionText = optionText });
            }

            var createdPoll = await _repository.CreatePollAsync(newPoll);
            var politician = await _repository.GetPoliticianByIdAsync(createPollDto.PoliticianTwitterId);
            return MapPollToDetailsDto(createdPoll, politician, null);
        }

        public async Task<List<PollSummaryDto>> GetAllPollsAsync()
        {
            return await _repository.GetAllPollsAsync();
        }

        public async Task<PollDetailsDto> GetPollByIdAsync(int id, int userId)
        {
            var poll = await _repository.GetPollByIdAsync(id);
            if (poll == null)
                return null;

            var userVote = await _repository.GetUserVoteAsync(id, userId);
            return MapPollToDetailsDto(poll, poll.Politician, userVote);
        }

        public async Task<bool> ValidatePoll(CreatePollDto pollDto)
        {
            var politician = await _repository.GetPoliticianByIdAsync(pollDto.PoliticianTwitterId);
            return politician != null && 
                  !pollDto.Options.Any(string.IsNullOrWhiteSpace) &&
                  pollDto.Options.Select(o => o.Trim().ToLowerInvariant()).Distinct().Count() == pollDto.Options.Count;
        }

        public async Task<bool> ValidateUpdatePoll(UpdatePollDto updateDto)
        {
            var politician = await _repository.GetPoliticianByIdAsync(updateDto.PoliticianTwitterId);
            return politician != null && 
                  !updateDto.Options.Any(string.IsNullOrWhiteSpace) &&
                  updateDto.Options.Select(o => o.Trim().ToLowerInvariant()).Distinct().Count() == updateDto.Options.Count;
        }

        public async Task<Poll> GetPollAsync(int id)
        {
            return await _repository.GetPollByIdAsync(id);
        }

        public async Task<bool> UpdatePollAsync(int id, UpdatePollDto updateDto)
        {
            var poll = await _repository.GetPollByIdAsync(id);
            if (poll == null)
                return false;

            poll.Question = updateDto.Question;
            poll.PoliticianTwitterId = updateDto.PoliticianTwitterId;
            poll.EndedAt = updateDto.EndedAt?.ToUniversalTime();

            poll.Options.Clear();
            foreach (var option in updateDto.Options)
            {
                poll.Options.Add(new PollOption { OptionText = option });
            }

            return await _repository.UpdatePollAsync(poll);
        }

        public async Task<(bool success, List<PollOptionDto> updatedOptions)> VoteAsync(int pollId, int userId, int optionId)
        {
            var success = await _repository.VoteAsync(pollId, userId, optionId);
            
            if (!success)
                return (false, null);

            var updatedPoll = await _repository.GetPollByIdAsync(pollId);
            var updatedOptions = updatedPoll.Options
                .OrderBy(o => o.Id)
                .Select(o => new PollOptionDto { 
                    Id = o.Id, 
                    OptionText = o.OptionText, 
                    Votes = o.Votes 
                })
                .ToList();
                
            return (true, updatedOptions);
        }

        public async Task<bool> DeletePollAsync(int id)
        {
            return await _repository.DeletePollAsync(id);
        }

        public async Task<PoliticianTwitterId> GetPolitician(int politicianId)
        {
            return await _repository.GetPoliticianByIdAsync(politicianId);
        }

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