// /backend/Controllers/Calendar/CalendarController.cs
namespace backend.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.Calendar;
using backend.Services.Calendar;
using backend.Services.Calendar.Scraping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
    public async Task<ActionResult<IEnumerable<CalendarEventDTO>>> GetEvents()
    {
        _logger.LogInformation("Attempting to fetch all calendar events via Service.");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

    // Bruger skal kunne deltage (subscribe) til kalenderevents
    [Authorize]
    [HttpPost("events/toggle-interest/{id}")]
    public async Task<ActionResult> ToggleInterest([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not authenticated or could not be retrieved.");
        }
        try
        {
            var result = await _calendarService.ToggleInterestAsync(id, userId);
            if (result.HasValue)
            {
                return Ok(new { isInterested = result.Value.IsInterested, interestedCount = result.Value.InterestedCount });
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
            _logger.LogError(ex, "An error occurred while fetching the number of interested users.");
            return StatusCode(500, "An internal error occurred while fetching the number of interested users.");
        }
    }
}
