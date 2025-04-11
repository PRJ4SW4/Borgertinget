// Controllers/CalendarController.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using backend.Data; // Your DbContext namespace
using backend.DTOs;
using backend.Models; // Your CalendarEvent entity namespace
using backend.Services.AutomationServices; // For AltingetScraperService (if keeping test endpoints)
using Microsoft.AspNetCore.Authorization; // If using authorization
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // For logging

[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly AltingetScraperService _scraperService;
    private readonly DataContext _context; // Inject DataContext
    private readonly ILogger<CalendarController> _logger; // Inject Logger

    // Inject the scraper service via constructor
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

    /// Manually triggers the Altinget calendar scrape and returns the raw results.
    /// FOR TESTING/DEBUGGING ONLY.
    // GET: api/calendar/scrape-altinget-now
    [HttpGet("scrape-altinget-now")]
    public async Task<ActionResult<List<ScrapedEventData>>> TriggerAltingetScrape()
    {
        try
        {
            // Directly call the scraping method
            List<ScrapedEventData> events = await _scraperService.ScrapeEventsAsyncInternal();

            // Return the list as JSON with a 200 OK status
            return Ok(events);
        }
        catch (Exception ex)
        {
            // Log the full exception server-side for debugging
            Console.WriteLine($"Error triggering scrape via API: {ex}");
            // Return a generic 500 Internal Server Error to the client
            return StatusCode(500, $"An error occurred while scraping data: {ex.Message}");
        }
    }

    /// Manually triggers the RunAutomation method (which includes console logging).
    /// FOR TESTING/DEBUGGING ONLY.
    // POST: api/test/run-altinget-automation
    [HttpPost("run-altinget-automation")] // Use POST as it conceptually "runs" a process
    public async Task<ActionResult> RunAutomationEndpoint()
    {
        try
        {
            int count = await _scraperService.RunAutomation(); // Call the implemented method
            if (count >= 0)
            {
                return Ok(
                    $"Scrape automation ran. Found/Processed {count} events. Check backend console logs for details."
                );
            }
            else
            {
                return StatusCode(500, "Scrape automation failed. Check backend console logs.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running scrape automation via API: {ex}");
            return StatusCode(500, "An error occurred while running scrape automation.");
        }
    }

    /// Gets stored calendar events, optionally filtered by a date range.
    /// Dates should be in ISO 8601 format (e.g., yyyy-MM-dd).
    // GET: api/calendar/events?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
    [HttpGet("events")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetEvents(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate
    )
    {
        _logger.LogInformation(
            "Attempting to fetch calendar events for range: Start='{StartDate}', End='{EndDate}'",
            startDate,
            endDate
        );

        try
        {
            // --- Parse Date Parameters ---
            // Treat input dates as representing the start of the day in UTC for filtering.
            DateTimeOffset? startFilterUtc = null;
            if (
                !string.IsNullOrEmpty(startDate)
                && DateTime.TryParse(
                    startDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedStartDate
                )
            )
            {
                // Use the start of the parsed day in UTC
                startFilterUtc = new DateTimeOffset(parsedStartDate.Date, TimeSpan.Zero);
            }

            DateTimeOffset? endFilterExclusiveUtc = null;
            if (
                !string.IsNullOrEmpty(endDate)
                && DateTime.TryParse(
                    endDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedEndDate
                )
            )
            {
                // Filter up to the *start* of the day *after* the specified end date
                endFilterExclusiveUtc = new DateTimeOffset(
                    parsedEndDate.Date.AddDays(1),
                    TimeSpan.Zero
                );
            }

            _logger.LogInformation(
                "Parsed filter range: StartUtc >= {StartFilterUtc}, EndUtc < {EndFilterExclusiveUtc}",
                startFilterUtc,
                endFilterExclusiveUtc
            );

            // --- Query Database ---
            // Start with the base query for CalendarEvents
            var query = _context.CalendarEvents.AsQueryable();

            // Apply start date filter if provided
            if (startFilterUtc.HasValue)
            {
                query = query.Where(e => e.StartDateTimeUtc >= startFilterUtc.Value);
            }

            // Apply end date filter if provided (exclusive end)
            if (endFilterExclusiveUtc.HasValue)
            {
                query = query.Where(e => e.StartDateTimeUtc < endFilterExclusiveUtc.Value);
            }

            // --- Execute Query and Map to DTO ---
            var events = await query
                .OrderBy(e => e.StartDateTimeUtc) // Order events chronologically
                .Select(e => new CalendarEventDto // Project to the DTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTimeUtc = e.StartDateTimeUtc, // Pass the UTC DateTimeOffset
                    Location = e.Location,
                    SourceUrl = e.SourceUrl,
                })
                .ToListAsync(); // Execute the database query

            _logger.LogInformation(
                "Found {EventCount} events for the specified range.",
                events.Count
            );
            return Ok(events); // Return the list of DTOs
        }
        catch (FormatException ex)
        {
            _logger.LogError(
                ex,
                "Error parsing date parameters: startDate='{StartDate}', endDate='{EndDate}'",
                startDate,
                endDate
            );
            // Return a 400 Bad Request for invalid date formats
            return BadRequest(
                "Invalid date format provided. Please use ISO 8601 format (e.g., yyyy-MM-dd)."
            );
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "An error occurred while fetching calendar events.");
            // Return a generic 500 Internal Server Error
            return StatusCode(500, "An internal error occurred while fetching events.");
        }
    }
    // ========== END NEW API ENDPOINT ==========
}
