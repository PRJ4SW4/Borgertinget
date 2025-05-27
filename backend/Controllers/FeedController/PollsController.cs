using System.Security.Claims;
using backend.DTOs;
using backend.Hubs;
using backend.Services.Polls;
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
        private readonly IPollsService _pollsService;
        private readonly IHubContext<FeedHub> _hubContext;
        private readonly ILogger<PollsController> _logger;

        public PollsController(
            IPollsService pollsService,
            IHubContext<FeedHub> hubContext,
            ILogger<PollsController> logger
        )
        {
            _pollsService = pollsService ?? throw new ArgumentNullException(nameof(pollsService));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PollDetailsDto>> CreatePoll(PollDto createPollDto)
        {
            if (createPollDto == null)
            {
                return BadRequest(
                    new ValidationProblemDetails
                    {
                        Title = "Invalid input",
                        Detail = "Poll data is required.",
                    }
                );
            }

            try
            {
                var createdPollDto = await _pollsService.CreatePollAsync(createPollDto);

                if (createdPollDto == null)
                {
                    return NotFound(
                        new ValidationProblemDetails
                        {
                            Title = "Creation failed",
                            Detail = "Failed to create poll.",
                        }
                    );
                }

                return CreatedAtAction(
                    nameof(GetPollById),
                    new { id = createdPollDto.Id },
                    createdPollDto
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving new poll");
                return StatusCode(
                    500,
                    new ValidationProblemDetails
                    {
                        Title = "Internal Server Error",
                        Detail = "An error occurred while creating the poll.",
                    }
                );
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PollSummaryDto>>> GetAllPolls()
        {
            try
            {
                var polls = await _pollsService.GetAllPollsAsync();
                return Ok(polls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af alle polls");
                return StatusCode(500, "Intern fejl ved hentning af polls.");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PollDetailsDto>> GetPollById(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);

            try
            {
                var pollDto = await _pollsService.GetPollByIdAsync(id, currentUserId);
                if (pollDto == null)
                    return NotFound();

                return Ok(pollDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fejl ved hentning af poll med id {id}");
                return StatusCode(500, "Intern fejl ved hentning af poll.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePoll(int id, PollDto updateDto)
        {
            var result = await _pollsService.UpdatePollAsync(id, updateDto);
            if (result == false)
                return NotFound("Poll blev ikke fundet.");
            return NoContent();
        }

        [HttpPost("{pollId}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote(int pollId, VoteDto voteDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);

            var poll = await _pollsService.GetPollAsync(pollId);
            if (poll == null)
                return NotFound("Afstemningen blev ikke fundet.");

            if (poll.EndedAt.HasValue && poll.EndedAt.Value < DateTime.UtcNow)
                return BadRequest("Afstemningen er afsluttet.");

            try
            {
                var (success, updatedOptions) = await _pollsService.VoteAsync(
                    pollId,
                    currentUserId,
                    voteDto.OptionId
                );

                if (!success)
                    return BadRequest("Ugyldig svarmulighed valgt.");

                var updatedPoll = await _pollsService.GetPollByIdAsync(pollId, currentUserId);
                if (updatedPoll == null)
                    return NotFound();
                var updatedOptionsData = updatedPoll
                    .Options.OrderBy(o => o.Id)
                    .Select(o => new { OptionId = o.Id, Votes = o.Votes })
                    .ToList();

                await _hubContext.Clients.All.SendAsync(
                    "PollVotesUpdated",
                    pollId,
                    updatedOptionsData
                );

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Fejl ved opdatering af stemme for poll {pollId}, bruger {currentUserId}, option {voteDto.OptionId}"
                );
                return StatusCode(500, "Intern fejl ved opdatering af stemme.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePoll(int id)
        {
            try
            {
                var success = await _pollsService.DeletePollAsync(id);
                if (!success)
                    return NotFound($"Poll med ID {id} blev ikke fundet.");

                await _hubContext.Clients.All.SendAsync("PollDeleted", id);

                return Ok(new { message = $"Poll med ID {id} blev slettet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fejl ved sletning af poll med id {id}");
                return StatusCode(500, "Intern fejl ved sletning af poll.");
            }
        }
    }
}
