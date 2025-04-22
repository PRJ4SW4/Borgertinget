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
using backend.Hubs;             // Using for FeedHub

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

        // --- Hent Enkelt Poll ---
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PollDetailsDto>> GetPollById(int id)
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId)) {
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

        // --- Afgiv Stemme ---
        [HttpPost("{pollId}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote(int pollId, VoteDto voteDto)
        {
            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            { return Unauthorized("Kunne ikke identificere brugeren."); }

            // Hent Poll OG dens Options i én query
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null) { return NotFound("Afstemningen blev ikke fundet."); }

            var chosenOption = poll.Options.FirstOrDefault(o => o.Id == voteDto.OptionId);
            if (chosenOption == null) { return BadRequest("Ugyldig svarmulighed valgt."); }
            if (poll.EndedAt.HasValue && poll.EndedAt.Value < DateTime.UtcNow) { return BadRequest("Afstemningen er afsluttet."); }

            bool alreadyVoted = await _context.UserVotes.AnyAsync(uv => uv.UserId == currentUserId && uv.PollId == pollId);
            if (alreadyVoted) { return Conflict("Du har allerede stemt på denne afstemning."); }

            var userVote = new UserVote { UserId = currentUserId, PollId = pollId, ChosenOptionId = voteDto.OptionId };
            chosenOption.Votes++;

            try
            {
                _context.UserVotes.Add(userVote);
                _context.Entry(chosenOption).State = EntityState.Modified; // Fortæl EF at Votes er ændret
                await _context.SaveChangesAsync(); // Gem både UserVote og opdateret Votes count

                // --- TRIGGER SIGNALR BROADCAST ---
                // Forbered data payload (en liste af objekter med option ID og nye stemmetal)
                var updatedOptionsData = poll.Options
                                            .OrderBy(o => o.Id) // Sorter for konsistens
                                            .Select(o => new { OptionId = o.Id, Votes = o.Votes })
                                            .ToList();

                // Send besked via Hub Context til ALLE forbundne klienter
                // Beskeden hedder "PollVotesUpdated"
                // Argumenterne er pollens ID og listen med opdaterede stemmetal
                await _hubContext.Clients.All.SendAsync("PollVotesUpdated", pollId, updatedOptionsData);
                // --------------------------------

                Console.WriteLine($"User {currentUserId} voted for option {voteDto.OptionId} on poll {pollId}. SignalR broadcast sent."); // Log success
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error saving vote: {dbEx}"); // TODO: Use ILogger
                // Overvej at fjerne userVote fra context hvis save fejlede? ELLER reload poll/option?
                return StatusCode(500, "Intern fejl ved afgivelse af stemme.");
            }
            catch (Exception ex) // Fang evt. fejl fra SignalR eller andet
            {
                 Console.WriteLine($"Error after saving vote (potentially SignalR): {ex}"); // TODO: Use ILogger
                 // Stemmen ER gemt, men broadcast fejlede måske. Returner OK for nu.
            }

            // Returner 200 OK til den klient, der stemte. Andre får opdatering via SignalR.
            return Ok();
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