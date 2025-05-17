namespace backend.Controllers;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.DTO.Calendar;
using backend.Services.Calendar;
using backend.Services.Calendar.Scraping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[ApiController]
// Defines the route for this controller, setting the base URL segment to "api/calendar".
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    // Private fields to hold injected services and dependencies.
    private readonly IScraperService _scraperService;
    private readonly ICalendarService _calendarService;
    private readonly ILogger<CalendarController> _logger;

    // Constructor for the CalendarController, injecting required services and dependencies.
    public CalendarController(
        IScraperService scraperService,
        ICalendarService calendarService,
        ILogger<CalendarController> logger
    )
    {
        // Assigns the injected IAutomationService instance to the private field.
        _scraperService = scraperService;
        // Assigns the injected ICalendarService instance to the private field.
        _calendarService = calendarService;
        // Assigns the injected ILogger instance to the private field.
        _logger = logger;
    }

    // Defines an HTTP POST endpoint for running the Altinget scrape automation.
    [HttpPost("run-calendar-scraper")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RunScraperEndpoint()
    {
        try
        {
            // Calls the RunAutomation method of the Automation Service to execute the automation process.
            int count = await _scraperService.RunScraper();
            // Checks if the automation ran successfully.
            if (count >= 0)
            {
                // Returns an HTTP 200 OK response with a message indicating the number of events found and processed.
                return Ok($"Scrape automation ran. Found/Processed {count} events.");
            }
            else
            {
                // Returns an HTTP 500 Internal Server Error response if the automation failed.
                return StatusCode(500, "Scrape automation failed.");
            }
        }
        catch (Exception ex)
        {
            // Logs any errors that occur during the automation process.
            _logger.LogError(ex, "Error running scrape automation via API.");
            // Returns an HTTP 500 Internal Server Error response with a generic error message.
            return StatusCode(500, "An error occurred while running scrape automation.");
        }
    }

    // Defines an HTTP GET endpoint for retrieving all calendar events.
    [HttpGet("events")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CalendarEventDTO>>> GetEvents()
    {
        _logger.LogInformation("Attempting to fetch all calendar events via Service.");

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User not authenticated or could not be retrieved.");
        }
        int.TryParse(userId, out int parsedUserId);

        try
        {
            var eventDTOs = await _calendarService.GetAllEventsAsDTOAsync(parsedUserId);
            return Ok(eventDTOs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching calendar events via Service.");
            return StatusCode(500, "An internal error occurred while fetching events.");
        }
    }

    #region Admin Endpoints Post/Put/Delete

    // Defines an HTTP POST endpoint for creating a new calendar event.
    [HttpPost("events")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CalendarEventDTO>> CreateEvent(
        [FromBody] CalendarEventDTO calendarEventDto
    )
    {
        // Logs an information message indicating that the attempt to create a new calendar event has started.
        _logger.LogInformation("Attempting to create a new calendar event via service.");

        if (string.IsNullOrEmpty(calendarEventDto.SourceUrl))
        {
            _logger.LogWarning("SourceUrl is required but was not provided.");
            return BadRequest("SourceUrl is required.");
        }

        try
        {
            var createdEventDto = await _calendarService.CreateEventAsync(calendarEventDto);

            // Returns an HTTP 201 Created response with the created event DTO.
            return CreatedAtAction(
                nameof(GetEvents),
                new { id = createdEventDto.Id },
                createdEventDto
            );
        }
        catch (Exception ex)
        {
            // Logs any errors that occur during the creation process.
            _logger.LogError(
                ex,
                "An error occurred while creating a new calendar event via service."
            );
            // Returns an HTTP 500 Internal Server Error response with a generic error message.
            return StatusCode(500, "An internal error occurred while creating the event.");
        }
    }

    // Defines an HTTP PUT endpoint for updating an existing calendar event.
    [HttpPut("events/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] CalendarEventDTO calendarEventDto
    )
    {
        _logger.LogInformation($"Attempting to update calendar event with ID: {id} via service.");

        if (id != calendarEventDto.Id)
        {
            _logger.LogWarning($"Mismatched ID in URL ({id}) and body ({calendarEventDto.Id}).");
            return BadRequest("ID in URL and body must match.");
        }

        if (string.IsNullOrEmpty(calendarEventDto.SourceUrl))
        {
            _logger.LogWarning($"SourceUrl is required but was not provided for event ID: {id}.");
            return BadRequest("SourceUrl is required.");
        }

        try
        {
            var success = await _calendarService.UpdateEventAsync(id, calendarEventDto);

            if (!success)
            {
                _logger.LogWarning(
                    $"Calendar event with ID: {id} not found for update via service, or update failed."
                );
                return NotFound(); // Or appropriate error if update failed for other reasons
            }

            _logger.LogInformation(
                $"Successfully updated calendar event with ID: {id} via service."
            );
            return NoContent(); // Or return Ok(calendarEventDto) if you want to return the updated object
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(
                ex,
                $"A concurrency error occurred while updating calendar event with ID: {id} via service."
            );
            return StatusCode(500, "A concurrency error occurred while updating the event.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"An error occurred while updating calendar event with ID: {id} via service."
            );
            return StatusCode(500, "An internal error occurred while updating the event.");
        }
    }

    // Defines an HTTP DELETE endpoint for deleting a calendar event.
    [HttpDelete("events/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        _logger.LogInformation($"Attempting to delete calendar event with ID: {id} via service.");

        try
        {
            var success = await _calendarService.DeleteEventAsync(id);

            if (!success)
            {
                _logger.LogWarning(
                    $"Calendar event with ID: {id} not found for deletion via service, or delete failed."
                );
                return NotFound();
            }

            _logger.LogInformation(
                $"Successfully deleted calendar event with ID: {id} via service."
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"An error occurred while deleting calendar event with ID: {id} via service."
            );
            return StatusCode(500, "An internal error occurred while deleting the event.");
        }
    }

    #endregion

    // Bruger skal kunne deltage (subscribe) til kalenderevents
    [Authorize]
    [HttpPost("events/toggle-interest/{id}")]
    public async Task<ActionResult> ToggleInterest([FromRoute] int id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User not authenticated or could not be retrieved.");
        }
        try
        {
            var result = await _calendarService.ToggleInterestAsync(id, userId);
            if (result.HasValue)
            {
                return Ok(
                    new
                    {
                        isInterested = result.Value.IsInterested,
                        interestedCount = result.Value.InterestedCount,
                    }
                );
            }
            else
            {
                return NotFound("Event not found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while toggling interest in the event.");
            return StatusCode(500, "An internal error occurred while toggling interest.");
        }
    }

    [Authorize]
    [HttpGet("events/get-amount-interested/{eventId}")]
    // Retrieves the number of users interested in a specific event.
    public async Task<ActionResult<int>> GetAmountInterested(int eventId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User not authenticated or could not be retrieved.");
        }
        try
        {
            var count = await _calendarService.GetAmountInterestedAsync(eventId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while fetching the number of interested users."
            );
            return StatusCode(
                500,
                "An internal error occurred while fetching the number of interested users."
            );
        }
    }

    private string? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            return userIdClaim.Value;
        }
        return null;
    }
}
