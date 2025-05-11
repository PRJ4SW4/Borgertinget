// /backend/Controllers/Calendar/CalendarController.cs
namespace backend.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.Calendar;
using backend.Models.Calendar;
using backend.Services.AutomationServices;
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
    private readonly AltingetScraperService _scraperService;
    private readonly DataContext _context;
    private readonly ILogger<CalendarController> _logger;

    // Constructor for the CalendarController, injecting required services and dependencies.
    public CalendarController(
        AltingetScraperService scraperService,
        DataContext context,
        ILogger<CalendarController> logger
    )
    {
        // Assigns the injected AltingetScraperService instance to the private field.
        _scraperService = scraperService;
        // Assigns the injected DataContext instance to the private field.
        _context = context;
        // Assigns the injected ILogger instance to the private field.
        _logger = logger;
    }

    // Defines an HTTP GET endpoint for triggering the Altinget scrape immediately.
    [HttpGet("scrape-altinget-now")]
    public async Task<ActionResult<List<ScrapedEventData>>> TriggerAltingetScrape()
    {
        try
        {
            // Calls the ScrapeEventsAsyncInternal method of the AltingetScraperService to perform the scrape.
            List<ScrapedEventData> events = await _scraperService.ScrapeEventsAsyncInternal();
            // Returns an HTTP 200 OK response containing the list of scraped events.
            return Ok(events);
        }
        catch (Exception ex)
        {
            // Logs any errors that occur during the scrape process.
            _logger.LogError(ex, "Error triggering raw scrape via API.");
            // Returns an HTTP 500 Internal Server Error response with a descriptive error message.
            return StatusCode(500, $"An error occurred while scraping data: {ex.Message}");
        }
    }

    // Defines an HTTP POST endpoint for running the Altinget scrape automation.
    [HttpPost("run-altinget-automation")]
    public async Task<ActionResult> RunAutomationEndpoint()
    {
        try
        {
            // Calls the RunAutomation method of the AltingetScraperService to execute the automation process.
            int count = await _scraperService.RunAutomation();
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
        // Logs an information message indicating that the attempt to fetch all calendar events has started.
        _logger.LogInformation("Attempting to fetch all calendar events.");

        try
        {
            // Creates a queryable object for the CalendarEvents entity set.
            var query = _context.CalendarEvents.AsQueryable();

            // Executes the database query and projects the results into CalendarEventDTO objects.
            var events = await query
                // Orders the events chronologically by their StartDateTimeUtc property.
                .OrderBy(e => e.StartDateTimeUtc)
                // Projects each CalendarEvent entity into a CalendarEventDTO object.
                .Select(e => new CalendarEventDTO
                {
                    // Maps the Id property from the CalendarEvent entity to the DTO.
                    Id = e.Id,
                    // Maps the Title property from the CalendarEvent entity to the DTO.
                    Title = e.Title,
                    // Maps the StartDateTimeUtc property from the CalendarEvent entity to the DTO.
                    StartDateTimeUtc = e.StartDateTimeUtc,
                    // Maps the Location property from the CalendarEvent entity to the DTO.
                    Location = e.Location,
                    // Maps the SourceUrl property from the CalendarEvent entity to the DTO.
                    SourceUrl = e.SourceUrl,
                })
                // Executes the database query and returns the results as a List.
                .ToListAsync();

            // Logs an information message indicating the number of events found.
            _logger.LogInformation("Found {EventCount} total events.", events.Count);
            // Returns an HTTP 200 OK response containing the list of CalendarEventDTO objects.
            return Ok(events);
        }
        catch (Exception ex)
        {
            // Logs any errors that occur during the process of fetching calendar events.
            _logger.LogError(ex, "An error occurred while fetching calendar events.");
            // Returns an HTTP 500 Internal Server Error response with a generic error message.
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
        _logger.LogInformation("Attempting to create a new calendar event.");

        if (string.IsNullOrEmpty(calendarEventDto.SourceUrl))
        {
            _logger.LogWarning("SourceUrl is required but was not provided.");
            return BadRequest("SourceUrl is required.");
        }

        try
        {
            // Creates a new CalendarEvent entity from the provided DTO.
            var calendarEvent = new CalendarEvent
            {
                Title = calendarEventDto.Title,
                StartDateTimeUtc = calendarEventDto.StartDateTimeUtc,
                Location = calendarEventDto.Location,
                SourceUrl = calendarEventDto.SourceUrl, // Already checked for null/empty
            };

            // Adds the new CalendarEvent entity to the database context.
            _context.CalendarEvents.Add(calendarEvent);
            // Saves the changes to the database.
            await _context.SaveChangesAsync();

            // Maps the created CalendarEvent entity back to a DTO for the response.
            var createdEventDto = new CalendarEventDTO
            {
                Id = calendarEvent.Id,
                Title = calendarEvent.Title,
                StartDateTimeUtc = calendarEvent.StartDateTimeUtc,
                Location = calendarEvent.Location,
                SourceUrl = calendarEvent.SourceUrl,
            };

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
            _logger.LogError(ex, "An error occurred while creating a new calendar event.");
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
        _logger.LogInformation($"Attempting to update calendar event with ID: {id}.");

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

        var calendarEvent = await _context.CalendarEvents.FindAsync(id);

        if (calendarEvent == null)
        {
            _logger.LogWarning($"Calendar event with ID: {id} not found for update.");
            return NotFound();
        }

        calendarEvent.Title = calendarEventDto.Title;
        calendarEvent.StartDateTimeUtc = calendarEventDto.StartDateTimeUtc;
        calendarEvent.Location = calendarEventDto.Location;
        calendarEvent.SourceUrl = calendarEventDto.SourceUrl; // Already checked for null/empty
        // LastScrapedUtc is not updated here as this is a manual override/edit

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully updated calendar event with ID: {id}.");
            return NoContent(); // Or return Ok(calendarEventDto) if you want to return the updated object
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(
                ex,
                $"A concurrency error occurred while updating calendar event with ID: {id}."
            );
            return StatusCode(500, "A concurrency error occurred while updating the event.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating calendar event with ID: {id}.");
            return StatusCode(500, "An internal error occurred while updating the event.");
        }
    }

    // Defines an HTTP DELETE endpoint for deleting a calendar event.
    [HttpDelete("events/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        _logger.LogInformation($"Attempting to delete calendar event with ID: {id}.");

        var calendarEvent = await _context.CalendarEvents.FindAsync(id);

        if (calendarEvent == null)
        {
            _logger.LogWarning($"Calendar event with ID: {id} not found for deletion.");
            return NotFound();
        }

        _context.CalendarEvents.Remove(calendarEvent);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully deleted calendar event with ID: {id}.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting calendar event with ID: {id}.");
            return StatusCode(500, "An internal error occurred while deleting the event.");
        }
    }

    #endregion
}
