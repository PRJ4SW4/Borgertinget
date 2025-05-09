// backend.Controllers/PollsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using backend.Hubs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<FeedHub> _hubContext; // HubContext til SignalR til polls bruger vi realtid, derfor er det i denne klasse blevet

        // injected fedhub ind i controlleren
        public PollsController(DataContext context, IHubContext<FeedHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Første endpoint er til at poste en ny poll, og der skal admin autorization til.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PollDetailsDto>> CreatePoll(CreatePollDto createPollDto) // modtager dto til oprettelse af pool
        {
            var politician = await _context
                .PoliticianTwitterIds.AsNoTracking()
                .FirstOrDefaultAsync(p => p.TwitterUserId == createPollDto.PoliticianTwitterId);
            if (politician == null)
            {
                ModelState.AddModelError(
                    nameof(createPollDto.PoliticianTwitterId),
                    "Den angivne politiker findes ikke."
                );
                return ValidationProblem(ModelState); // hvis den modtagne twitter id ikke er i i db, sendes en Validation problem
            }

            if (createPollDto.Options.Any(string.IsNullOrWhiteSpace))
            {
                ModelState.AddModelError(
                    nameof(createPollDto.Options),
                    "Svarmuligheder må ikke være tomme."
                ); // kontrolere om nogler af "data" felterne er tomme
                return ValidationProblem(ModelState);
            }

            var distinctOptions = createPollDto
                .Options.Select(o => o.Trim().ToLowerInvariant())
                .Distinct()
                .Count();
            if (distinctOptions != createPollDto.Options.Count) // disse 2 linjer sikrer, at svarmulighederne ikke er de samme.
            {
                ModelState.AddModelError(
                    nameof(createPollDto.Options),
                    "Svarmuligheder må ikke være ens."
                );
                return ValidationProblem(ModelState);
            }

            var newPoll = new Poll // her oprettes poolen, tid sættes til UTC og endtime sættes til den tid der er i dtoen
            // pollotion objekter bliver tilføjet som valgmuligheder til pollen
            {
                Question = createPollDto.Question,
                PoliticianTwitterId = createPollDto.PoliticianTwitterId,
                CreatedAt = DateTime.UtcNow,
                EndedAt = createPollDto.EndedAt?.ToUniversalTime(),
            };

            foreach (var optionText in createPollDto.Options)
            {
                newPoll.Options.Add(new PollOption { OptionText = optionText });
            }

            try
            {
                _context.Polls.Add(newPoll); // gemmer pollen i databasen
                await _context.SaveChangesAsync();
                var createdPollDto = MapPollToDetailsDto(newPoll, politician, null);
                return CreatedAtAction(
                    nameof(GetPollById),
                    new { id = newPoll.Id },
                    createdPollDto
                );
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Fejl ved gemning af ny poll: {ex}");
                return StatusCode(500, "Intern fejl ved oprettelse af poll.");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PollSummaryDto>>> GetAllPolls()
        {
            var polls = await _context
                .Polls.AsNoTracking()
                .Select(p => new PollSummaryDto
                {
                    Id = p.Id,
                    Question = p.Question,
                    PoliticianTwitterId = p.PoliticianTwitterId!,
                })
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(polls);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PollDetailsDto>> GetPollById(int id)
        {
            var userIdString = User.FindFirstValue("userId");
            if (
                string.IsNullOrEmpty(userIdString)
                || !int.TryParse(userIdString, out int currentUserId)
            ) // her tjekker vi om brugeren er logget ind og om den har et gyldigt id
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            // simpelt, så henter den bare pool med det id der er givet i igt. endpointer

            var poll = await _context
                .Polls.Include(p => p.Options.OrderBy(o => o.Id))
                .Include(p => p.Politician)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null)
            {
                return NotFound();
            }

            var userVote = await _context
                .UserVotes // her tjekkes om brugeren har stemt på poll, og hvis den har, så hentes den stemme
                .AsNoTracking()
                .FirstOrDefaultAsync(uv => uv.PollId == id && uv.UserId == currentUserId);

            var pollDto = MapPollToDetailsDto(poll, poll.Politician, userVote); // mapper poll til PollDetailsDto, så den kan sendes til frontend
            return Ok(pollDto);
        }

        // Nyt endpoint til at opdatere en poll
        // Kun for admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePoll(int id, UpdatePollDto updateDto)
        {
            var poll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null)
                return NotFound();

            var politician = await _context.PoliticianTwitterIds.FirstOrDefaultAsync(p =>
                p.TwitterUserId == updateDto.PoliticianTwitterId
            );

            if (politician == null)
            {
                ModelState.AddModelError(
                    nameof(updateDto.PoliticianTwitterId),
                    "Politikeren findes ikke."
                );
                return ValidationProblem(ModelState);
            }

            if (updateDto.Options.Any(string.IsNullOrWhiteSpace))
            {
                ModelState.AddModelError(
                    nameof(updateDto.Options),
                    "Svarmuligheder må ikke være tomme."
                );
                return ValidationProblem(ModelState);
            }

            if (
                updateDto.Options.Select(o => o.Trim().ToLowerInvariant()).Distinct().Count()
                != updateDto.Options.Count
            )
            {
                ModelState.AddModelError(
                    nameof(updateDto.Options),
                    "Svarmuligheder må ikke være ens."
                );
                return ValidationProblem(ModelState);
            }

            // Update fields
            poll.Question = updateDto.Question;
            poll.PoliticianTwitterId = updateDto.PoliticianTwitterId;
            poll.PoliticianId = politician.Id;
            poll.EndedAt = updateDto.EndedAt?.ToUniversalTime();

            // Replace all options
            poll.Options.Clear();
            foreach (var option in updateDto.Options)
            {
                poll.Options.Add(new PollOption { OptionText = option });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // denne endpoint opdaterer spørgsmålet på en poll så det vil sige (spørgsmål option option option option)
        // endpoint ændrer derfor kun spørgsmål kun
        [HttpPut("{id}/question")]
        [Authorize]
        public async Task<ActionResult<string>> UpdatePollQuestion(
            int id,
            [FromBody] PollQuestionUpdate update
        )
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
        // endpoint ændrer derfor kun den option der vælges, likes, forbliver det samme

        [HttpPut("{pollId}/options/{optionId}")]
        [Authorize]
        public async Task<ActionResult> UpdatePollOption(
            int pollId,
            int optionId,
            [FromBody] PollOptionUpdate update
        )
        {
            // først findes pollId og derfor option 1d i databasen.
            if (string.IsNullOrWhiteSpace(update.NewOptionText))
            {
                return BadRequest("Option text must not be empty.");
            }

            var poll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null)
            {
                return NotFound($"Poll with ID {pollId} not found.");
            }

            var pollOption = await _context.PollOptions.FirstOrDefaultAsync(o =>
                o.Id == optionId && o.PollId == pollId
            );
            if (pollOption == null)
            {
                return NotFound($"Option with ID {optionId} not found for Poll with ID {pollId}.");
            }

            pollOption.OptionText = update.NewOptionText;

            try
            {
                await _context.SaveChangesAsync(); // gemmer ændringerne i databasen
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
            // Tjek om brugeren er logget ind og har et gyldigt userId
            var userIdString = User.FindFirstValue("userId");
            if (
                string.IsNullOrEmpty(userIdString)
                || !int.TryParse(userIdString, out int currentUserId)
            )
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            // henter polls og options fra databasen
            var poll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId); // note til mig selv, så betyder det at den henter pollId fra databasen og derefter optionsne til den pollId eller null hvis de ikke findes

            // Logik til til hvis poll er null eller hvis poll er afsluttet.
            if (poll == null)
            {
                return NotFound("Afstemningen blev ikke fundet.");
            }
            if (poll.EndedAt.HasValue && poll.EndedAt.Value < DateTime.UtcNow)
            {
                return BadRequest("Afstemningen er afsluttet.");
            }

            // Tjek om den valgte option findes i optionsne til den pollId i db
            var chosenOption = poll.Options.FirstOrDefault(o => o.Id == voteDto.OptionId);
            if (chosenOption == null)
            {
                return BadRequest("Ugyldig svarmulighed valgt.");
            }

            var existingVote = await _context.UserVotes.FirstOrDefaultAsync(uv =>
                uv.UserId == currentUserId && uv.PollId == pollId
            );

            try
            { // logik for stemning, der tjekkes om brugeren har stemt før og om den stemme der vælges er den samme som før
                if (existingVote == null)
                {
                    var userVote = new UserVote
                    {
                        UserId = currentUserId,
                        PollId = pollId,
                        ChosenOptionId = voteDto.OptionId,
                    };
                    chosenOption.Votes++; // Tæl den nye stemme op
                    _context.UserVotes.Add(userVote);
                    _context.Entry(chosenOption).State = EntityState.Modified;
                }
                else // Bruger har stemt før, så skal vi opdatere stemme istedet.
                {
                    if (existingVote.ChosenOptionId == voteDto.OptionId)
                    {
                        // hvis brugeren trykker på den samme option i frontend, så skal den ikke ændres
                        // og der skal ikke ske noget i databasen, så returneres bare en besked
                        return Ok("Stemme ikke ændret.");
                    }

                    // brugeren stemmer på en anden
                    var oldOption = poll.Options.FirstOrDefault(o =>
                        o.Id == existingVote.ChosenOptionId
                    );

                    if (oldOption != null)
                    {
                        oldOption.Votes--; // Træk 1 fra den gamle
                        _context.Entry(oldOption).State = EntityState.Modified;
                    }
                    else
                    {
                        // warning hvis den gamle option ikke findes i databasen
                        Console.WriteLine(
                            $"Warning: Old option (ID: {existingVote.ChosenOptionId}) not found for vote change."
                        );
                    }

                    chosenOption.Votes++; // Læg 1 til den nye
                    existingVote.ChosenOptionId = voteDto.OptionId; // Opdater den valgte option i UserVote
                    _context.Entry(chosenOption).State = EntityState.Modified;
                    _context.Entry(existingVote).State = EntityState.Modified;
                }

                // Gem ændringerne i dben
                await _context.SaveChangesAsync();

                //SIGNALR BROADCAST
                var updatedOptionsData = poll
                    .Options.OrderBy(o => o.Id)
                    .Select(o => new { OptionId = o.Id, Votes = o.Votes })
                    .ToList();
                await _hubContext.Clients.All.SendAsync(
                    "PollVotesUpdated",
                    pollId,
                    updatedOptionsData
                ); // Send SignalR-besked til alle klienter i front enden om, at afstemningen er opdateret
                // -------------------------------------------
                Console.WriteLine(
                    $"User {currentUserId} processed vote/vote change for option {voteDto.OptionId} on poll {pollId}. SignalR broadcast sent."
                );
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Fejl ved opdatering af stemme: {dbEx}");
                return StatusCode(500, "Intern fejl ved opdatering af stemme.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved opdatering af stemme: {ex}");
                return StatusCode(500, "Intern fejl ved opdatering af stemme.");
            }

            return Ok();
        }

        // Denne endpoint sletter en poll og dens tilhørende options og stemmer
        // Den tjekker først om pollId findes i databasen og derefter om den har stemmer, hvis den har, så slettes de også.
        // Til sidst slettes pollId og dens options fra databasen.
        // Hvis pollId ikke findes, returneres en 404 fejl.

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var poll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null)
            {
                return NotFound($"Poll med ID {id} blev ikke fundet.");
            }

            try
            {
                var votes = await _context.UserVotes.Where(uv => uv.PollId == id).ToListAsync();

                if (votes.Any())
                {
                    _context.UserVotes.RemoveRange(votes);
                }

                // slette alle poll options
                _context.PollOptions.RemoveRange(poll.Options);

                // slette poll itself
                _context.Polls.Remove(poll);

                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("PollDeleted", id);

                return Ok(new { message = $"Poll med ID {id} blev slettet." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved sletning af poll: {ex}");
                return StatusCode(500, "Intern fejl ved sletning af poll.");
            }
        }

        //Hjælpemetode til Mapping

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
                // IsActive beregnes i DTO'en
                PoliticianId = politician.Id,
                PoliticianName = politician.Name,
                PoliticianHandle = politician.TwitterHandle, // Sørg for at dette felt findes på PoliticianTwitterId modellen
                Options =
                    poll.Options? // Brug null-conditional operator
                        .Select(o => new PollOptionDto
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
