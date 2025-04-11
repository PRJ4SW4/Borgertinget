// Controllers/CalendarController.cs
using System;
using System.Collections.Generic;
// using System.Globalization; // No longer needed for parsing here
using System.Linq;
using System.Threading.Tasks;
using backend.Data; // Your DbContext namespace
using backend.DTOs; // Your DTO namespace
using backend.Models; // Your CalendarEvent entity namespace
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly AltingetScraperService _scraperService;
    private readonly DataContext _context;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(
        AltingetScraperService scraperService,
        DataContext context,
        ILogger<CalendarController> logger
    )
    {
        _scraperService = scraperService;
        _context = context;
        _logger = logger;
    }

    // --- Test Endpoints (Keep as they were) ---
    [HttpGet("scrape-altinget-now")]
    public async Task<ActionResult<List<ScrapedEventData>>> TriggerAltingetScrape()
    {
        try
        {
            // NOTE: Ensure ScrapedEventData is defined (likely in AltingetScraperService.cs)
            List<ScrapedEventData> events = await _scraperService.ScrapeEventsAsyncInternal();
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering raw scrape via API.");
            return StatusCode(500, $"An error occurred while scraping data: {ex.Message}");
        }
    }

    [HttpPost("run-altinget-automation")]
    public async Task<ActionResult> RunAutomationEndpoint()
    {
        try
        {
            int count = await _scraperService.RunAutomation();
            if (count >= 0)
            {
                return Ok($"Scrape automation ran. Found/Processed {count} events.");
            }
            else
            {
                return StatusCode(500, "Scrape automation failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running scrape automation via API.");
            return StatusCode(500, "An error occurred while running scrape automation.");
        }
    }

    // --- End Test Endpoints ---


    // ========== MODIFIED API ENDPOINT ==========
    /// <summary>
    /// Gets all stored calendar events.
    /// </summary>
    /// <returns>A list of all calendar events.</returns>
    // GET: api/calendar/events
    [HttpGet("events")]
    // [Authorize(Policy = "UserOrAdmin")] // Add Auth if needed
    // Removed startDate and endDate parameters
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetEvents()
    {
        _logger.LogInformation("Attempting to fetch all calendar events."); // Updated log message

        try
        {
            // --- Removed Date Parameter Parsing Logic ---

            // --- Query Database (No date filtering) ---
            var query = _context.CalendarEvents.AsQueryable();

            // --- Removed .Where() clauses for filtering ---

            // --- Execute Query and Map to DTO ---
            var events = await query
                .OrderBy(e => e.StartDateTimeUtc) // Order events chronologically
                .Select(e => new CalendarEventDto // Project to the DTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTimeUtc = e.StartDateTimeUtc,
                    Location = e.Location,
                    SourceUrl = e.SourceUrl,
                })
                .ToListAsync(); // Execute the database query

            _logger.LogInformation("Found {EventCount} total events.", events.Count); // Updated log message
            return Ok(events); // Return the list of DTOs
        }
        // Removed FormatException catch as parsing is removed
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching calendar events.");
            return StatusCode(500, "An internal error occurred while fetching events.");
        }
    }
    // ========== END MODIFIED API ENDPOINT ==========
}
