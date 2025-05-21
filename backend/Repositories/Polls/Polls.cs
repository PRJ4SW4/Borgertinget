using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Polls
{
    public class PollsRepository : IPollsRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<PollsRepository> _logger;

        public PollsRepository(DataContext context, ILogger<PollsRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Poll> CreatePollAsync(Poll poll)
        {
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();
            return poll;
        }

        public async Task<List<PollSummaryDto>> GetAllPollsAsync()
        {
            return await _context
                .Polls.AsNoTracking()
                .Select(p => new PollSummaryDto
                {
                    Id = p.Id,
                    Question = p.Question,
                    PoliticianTwitterId = p.PoliticianTwitterId!,
                })
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<Poll?> GetPollByIdAsync(int id)
        {
            return await _context
                .Polls.Include(p => p.Options.OrderBy(o => o.Id))
                .Include(p => p.Politician)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<UserVote?> GetUserVoteAsync(int pollId, int userId)
        {
            return await _context
                .UserVotes.AsNoTracking()
                .FirstOrDefaultAsync(uv => uv.PollId == pollId && uv.UserId == userId);
        }

        public async Task<PoliticianTwitterId?> GetPoliticianByIdAsync(int politicianId)
        {
            return await _context
                .PoliticianTwitterIds.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == politicianId);
        }

        public async Task<bool> UpdatePollAsync(Poll poll)
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Fejl ved opdatering af poll med id {poll.Id}");
                throw;
            }
        }

        public async Task<bool> VoteAsync(int pollId, int userId, int optionId)
        {
            var poll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll == null)
                return false;

            var chosenOption = poll.Options.FirstOrDefault(o => o.Id == optionId);
            if (chosenOption == null)
                return false;

            var existingVote = await _context.UserVotes.FirstOrDefaultAsync(uv =>
                uv.UserId == userId && uv.PollId == pollId
            );

            try
            {
                if (existingVote == null)
                {
                    var userVote = new UserVote
                    {
                        UserId = userId,
                        PollId = pollId,
                        ChosenOptionId = optionId,
                    };
                    chosenOption.Votes++;
                    _context.UserVotes.Add(userVote);
                    _context.Entry(chosenOption).State = EntityState.Modified;
                }
                else
                {
                    if (existingVote.ChosenOptionId == optionId)
                        return true;

                    var oldOption = poll.Options.FirstOrDefault(o =>
                        o.Id == existingVote.ChosenOptionId
                    );
                    if (oldOption != null)
                    {
                        oldOption.Votes--;
                        _context.Entry(oldOption).State = EntityState.Modified;
                    }

                    chosenOption.Votes++;
                    existingVote.ChosenOptionId = optionId;
                    _context.Entry(chosenOption).State = EntityState.Modified;
                    _context.Entry(existingVote).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Error updating vote for poll {pollId}, user {userId}, option {optionId}"
                );
                throw;
            }
        }

        public async Task<bool> DeletePollAsync(int id)
        {
            var poll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null)
                return false;

            var votes = await _context.UserVotes.Where(uv => uv.PollId == id).ToListAsync();

            if (votes.Any())
            {
                _context.UserVotes.RemoveRange(votes);
            }

            _context.PollOptions.RemoveRange(poll.Options);
            _context.Polls.Remove(poll);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeletePoll(Poll poll)
        {
            var votes = await _context.UserVotes.Where(uv => uv.PollId == poll.Id).ToListAsync();

            if (votes.Any())
            {
                _context.UserVotes.RemoveRange(votes);
            }

            _context.PollOptions.RemoveRange(poll.Options);
            _context.Polls.Remove(poll);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
