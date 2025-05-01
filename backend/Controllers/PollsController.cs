// backend.Controllers/PollsController.cs
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR; // Using for SignalR
using backend.Hubs;                // Using for FeedHub

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<FeedHub> _hubContext; // Inject Hub Context

        public PollsController(DataContext context, IHubContext<FeedHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; // Assign injected context
        }

        // --- Opret Poll ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PollDetailsDto>> CreatePoll(CreatePollDto createPollDto)
        {
            var politician = await _context.PoliticianTwitterIds
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(p => p.Id == createPollDto.PoliticianTwitterId);
            if (politician == null)
            {
                ModelState.AddModelError(nameof(createPollDto.PoliticianTwitterId), "Den angivne politiker findes ikke.");
                return ValidationProblem(ModelState);
            }

            if (createPollDto.Options.Any(string.IsNullOrWhiteSpace))
            {
                ModelState.AddModelError(nameof(createPollDto.Options), "Svarmuligheder må ikke være tomme.");
                return ValidationProblem(ModelState);
            }

            var distinctOptions = createPollDto.Options.Select(o => o.Trim().ToLowerInvariant()).Distinct().Count();
            if (distinctOptions != createPollDto.Options.Count)
            {
                ModelState.AddModelError(nameof(createPollDto.Options), "Svarmuligheder må ikke være ens.");
                return ValidationProblem(ModelState);
            }

            var newPoll = new Poll
            {
                Question = createPollDto.Question,
                PoliticianTwitterId = createPollDto.PoliticianTwitterId,
                CreatedAt = DateTime.UtcNow,
                EndedAt = createPollDto.EndedAt?.ToUniversalTime()
            };

            foreach (var optionText in createPollDto.Options)
            {
                newPoll.Options.Add(new PollOption { OptionText = optionText });
            }

            try
            {
                _context.Polls.Add(newPoll);
                await _context.SaveChangesAsync();
                var createdPollDto = MapPollToDetailsDto(newPoll, politician, null);
                return CreatedAtAction(nameof(GetPollById), new { id = newPoll.Id }, createdPollDto);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Fejl ved gemning af ny poll: {ex}"); // TODO: Brug ILogger
                return StatusCode(500, "Intern fejl ved oprettelse af poll.");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PollDetailsDto>> GetPollById(int id)
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId)) 
            {
                // Overvej at returnere data uden CurrentUserVoteOptionId hvis bruger ikke er logget ind
                // eller hvis det er okay at se polls anonymt. For nu kræver vi login.
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            var poll = await _context.Polls
                .Include(p => p.Options.OrderBy(o => o.Id)) // Sorter options her
                .Include(p => p.Politician)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null) { return NotFound(); }

            var userVote = await _context.UserVotes
                .AsNoTracking()
                .FirstOrDefaultAsync(uv => uv.PollId == id && uv.UserId == currentUserId);

            var pollDto = MapPollToDetailsDto(poll, poll.Politician, userVote);
            return Ok(pollDto);
        }

        // denne endpoint opdaterer spørgsmålet på en poll så det vil sige (spørgsmål option option option option) 
        // endpoint ændrer derfor kun spørgsmål kun
        [HttpPut("{id}/question")]
        [Authorize]
        public async Task<ActionResult<string>> UpdatePollQuestion(int id, [FromBody] PollQuestionUpdate update)
        {
            if (string.IsNullOrWhiteSpace(update.NewQuestion))
            {
                return BadRequest("Spørgsmålet må ikke være tomt.");
            }

            var poll = await _context.Polls.FindAsync(id);
            if (poll == null)
            {
                return NotFound($"Poll med ID {id} blev ikke fundet.");
            }

            // Check if poll has ended
            if (poll.EndedAt.HasValue && poll.EndedAt.Value < DateTime.UtcNow)
            {
                return BadRequest("Kan ikke redigere en afsluttet poll.");
            }

            // Just update the question
            poll.Question = update.NewQuestion;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Spørgsmålet blev opdateret", question = poll.Question });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved opdatering af poll spørgsmål: {ex}");
                return StatusCode(500, "Intern fejl ved opdatering af poll spørgsmål.");
            }
        }


         // denne endpoint opdaterer option på en poll så det vil sige (spørgsmål option option option option) 
        // endpoint ændrer derfor kun option kun, likes, forbliver det samme

        [HttpPut("{pollId}/options/{optionId}")]
        [Authorize]
        public async Task<ActionResult> UpdatePollOption(int pollId, int optionId, [FromBody] PollOptionUpdate update)
        {
            if (string.IsNullOrWhiteSpace(update.NewOptionText))
            {
                return BadRequest("Option text must not be empty.");
            }

            // Find the poll by its ID and include its options to check relationships
            var poll = await _context.Polls
                                    .Include(p => p.Options) // Ensure we load options as well
                                    .FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null)
            {
                return NotFound($"Poll with ID {pollId} not found.");
            }

            // Find the option by its ID within the poll
            var pollOption = await _context.PollOptions
                                        .FirstOrDefaultAsync(o => o.Id == optionId && o.PollId == pollId);
            if (pollOption == null)
            {
                return NotFound($"Option with ID {optionId} not found for Poll with ID {pollId}.");
            }

            pollOption.OptionText = update.NewOptionText;

            try
            {
                await _context.SaveChangesAsync(); // Commit the changes to the database
                return Ok(new { message = "Poll option updated", option = pollOption });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating poll option: {ex}");
                return StatusCode(500, "Internal server error while updating poll option.");
            }
        }

        [HttpPost("{pollId}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote(int pollId, VoteDto voteDto) // voteDto indeholder int OptionId
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            { 
                return Unauthorized("Kunne ikke identificere brugeren."); 
            }

            // Find poll MED options OG den eksisterende stemme (hvis den findes) i én query
            var poll = await _context.Polls
                .Include(p => p.Options) // Vigtigt for at have adgang til options
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll == null) { return NotFound("Afstemningen blev ikke fundet."); }
            if (poll.EndedAt.HasValue && poll.EndedAt.Value < DateTime.UtcNow) { return BadRequest("Afstemningen er afsluttet."); }

            var chosenOption = poll.Options.FirstOrDefault(o => o.Id == voteDto.OptionId);
            if (chosenOption == null) { return BadRequest("Ugyldig svarmulighed valgt."); }

            // Find brugerens EKSISTERENDE stemme på denne poll (hvis nogen)
            var existingVote = await _context.UserVotes
                                .FirstOrDefaultAsync(uv => uv.UserId == currentUserId && uv.PollId == pollId);

            try
            {
                if (existingVote == null) // Bruger har IKKE stemt før = Opret ny stemme
                {
                    var userVote = new UserVote { UserId = currentUserId, PollId = pollId, ChosenOptionId = voteDto.OptionId };
                    chosenOption.Votes++; // Tæl den nye stemme op
                    _context.UserVotes.Add(userVote);
                    _context.Entry(chosenOption).State = EntityState.Modified;
                }
                else // Bruger HAR stemt før = Ændr stemme
                {
                    if (existingVote.ChosenOptionId == voteDto.OptionId)
                    {
                        // Brugeren klikkede på den samme option igen - gør intet
                        // (Alternativt kunne man her implementere "fjern stemme")
                        return Ok("Stemme ikke ændret.");
                    }

                    // Find den gamle option brugeren stemte på
                    var oldOption = poll.Options.FirstOrDefault(o => o.Id == existingVote.ChosenOptionId);

                    if (oldOption != null)
                    {
                        oldOption.Votes--; // Træk 1 fra den gamle
                        _context.Entry(oldOption).State = EntityState.Modified;
                    } 
                    else 
                    {
                        // Burde ikke ske hvis data er konsistent, men håndter evt.
                        Console.WriteLine($"Warning: Old option (ID: {existingVote.ChosenOptionId}) not found for vote change.");
                    }

                    chosenOption.Votes++; // Læg 1 til den nye
                    existingVote.ChosenOptionId = voteDto.OptionId; // Opdater UserVote record

                    _context.Entry(chosenOption).State = EntityState.Modified;
                    _context.Entry(existingVote).State = EntityState.Modified;
                }

                // Gem ændringerne (enten ny stemme eller ændret stemme)
                await _context.SaveChangesAsync();

                // --- TRIGGER SIGNALR BROADCAST (som før) ---
                var updatedOptionsData = poll.Options
                                        .OrderBy(o => o.Id)
                                        .Select(o => new { OptionId = o.Id, Votes = o.Votes })
                                        .ToList();
                await _hubContext.Clients.All.SendAsync("PollVotesUpdated", pollId, updatedOptionsData);
                // -------------------------------------------
                Console.WriteLine($"User {currentUserId} processed vote/vote change for option {voteDto.OptionId} on poll {pollId}. SignalR broadcast sent.");
            }
            catch (DbUpdateException dbEx) { /* ... fejlhåndtering ... */ }
            catch (Exception ex) { /* ... fejlhåndtering ... */ }

            return Ok(); 
        }

        // endpoint her sletter poll og dens options og votes
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (poll == null)
            {
                return NotFound($"Poll med ID {id} blev ikke fundet.");
            }

            try
            {
                // Remove related votes first
                var votes = await _context.UserVotes
                    .Where(uv => uv.PollId == id)
                    .ToListAsync();
                    
                if (votes.Any())
                {
                    _context.UserVotes.RemoveRange(votes);
                }
                
                // Remove poll options
                _context.PollOptions.RemoveRange(poll.Options);
                
                // Remove the poll itself
                _context.Polls.Remove(poll);
                
                await _context.SaveChangesAsync();
                
               
                await _hubContext.Clients.All.SendAsync("PollDeleted", id);
                
                return Ok(new { message = $"Poll med ID {id} blev slettet." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved sletning af poll: {ex}"); // TODO: Brug ILogger
                return StatusCode(500, "Intern fejl ved sletning af poll.");
            }
        }

        // --- Privat Hjælpemetode til Mapping ---
        private PollDetailsDto MapPollToDetailsDto(Poll poll, PoliticianTwitterId politician, UserVote? userVote)
        {
            // Sørg for at poll.Options er loadet (hvilket den er pga. .Include i GetPollById)
            int totalVotes = poll.Options?.Sum(o => o.Votes) ?? 0; // Brug null-conditional operator
            return new PollDetailsDto
            {
                Id = poll.Id,
                Question = poll.Question,
                CreatedAt = poll.CreatedAt,
                EndedAt = poll.EndedAt,
                // IsActive beregnes i DTO'en
                PoliticianId = politician.Id,
                PoliticianName = politician.Name,
                PoliticianHandle = politician.TwitterHandle, // Sørg for at dette felt findes på PoliticianTwitterId modellen
                Options = poll.Options? // Brug null-conditional operator
                    .Select(o => new PollOptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        Votes = o.Votes
                        // Her kunne man udregne VotePercentage hvis ønsket
                        // VotePercentage = totalVotes == 0 ? 0 : Math.Round((double)o.Votes / totalVotes * 100, 1)
                    }).OrderBy(o => o.Id).ToList() ?? new List<PollOptionDto>(), // Returner tom liste hvis options er null
                CurrentUserVoteOptionId = userVote?.ChosenOptionId,
                TotalVotes = totalVotes
            };
        }
    }
}