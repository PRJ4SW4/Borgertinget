using backend.DTOs;
using backend.Models;
using backend.Repositories.Polls;

namespace backend.Services.Polls
{
    public class PollsService : IPollsService
    {
        private readonly IPollsRepository _repository;

        public PollsService(IPollsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<PollDetailsDto> CreatePollAsync(PollDto createPollDto)
        {
            var newPoll = new Poll
            {
                Question = createPollDto.Question,
                PoliticianTwitterId = createPollDto.PoliticianTwitterId,
                CreatedAt = DateTime.UtcNow,
                EndedAt = createPollDto.EndedAt?.ToUniversalTime(),
                Options = new List<PollOption>(),
            };

            foreach (var optionText in createPollDto.Options)
            {
                newPoll.Options.Add(new PollOption { OptionText = optionText });
            }

            var createdPoll = await _repository.CreatePollAsync(newPoll);
            var politician =
                await _repository.GetPoliticianByIdAsync(createPollDto.PoliticianTwitterId)
                ?? new PoliticianTwitterId();
            return MapPollToDetailsDto(createdPoll, politician, null);
        }

        public async Task<List<PollSummaryDto>> GetAllPollsAsync()
        {
            return await _repository.GetAllPollsAsync();
        }

        public async Task<PollDetailsDto?> GetPollByIdAsync(int id, int userId)
        {
            var poll = await _repository.GetPollByIdAsync(id);
            if (poll == null)
                return null;

            var userVote = await _repository.GetUserVoteAsync(id, userId);
            return MapPollToDetailsDto(
                poll,
                poll.Politician ?? new PoliticianTwitterId(),
                userVote
            );
        }

        public async Task<Poll> GetPollAsync(int id)
        {
            return await _repository.GetPollByIdAsync(id) ?? new Poll();
        }

        public async Task<bool> UpdatePollAsync(int id, PollDto updateDto)
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

        public async Task<(bool success, List<PollOptionDto> updatedOptions)> VoteAsync(
            int pollId,
            int userId,
            int optionId
        )
        {
            var poll = await _repository.GetPollByIdAsync(pollId);
            if (poll == null)
                return (false, new List<PollOptionDto>());

            var success = await _repository.VoteAsync(pollId, userId, optionId);
            if (!success)
                return (false, new List<PollOptionDto>());

            var updatedOptions =
                poll.Options?.Select(o => new PollOptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        Votes = o.Votes,
                    })
                    .ToList() ?? new List<PollOptionDto>();

            return (true, updatedOptions);
        }

        public async Task<bool> DeletePollAsync(int id)
        {
            var poll = await _repository.GetPollByIdAsync(id);
            if (poll == null)
                return false;
            await _repository.DeletePollAsync(poll);
            int changes = await _repository.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<PoliticianTwitterId> GetPolitician(int politicianId)
        {
            return await _repository.GetPoliticianByIdAsync(politicianId)
                ?? new PoliticianTwitterId();
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
