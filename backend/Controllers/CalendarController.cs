// Controllers/CalendarController.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly AltingetScraperService _scraperService;

    // Inject the scraper service via constructor
    public CalendarController(AltingetScraperService scraperService)
    {
        _scraperService = scraperService;
    }

    /// Manually triggers the Altinget calendar scrape and returns the raw results.
    /// FOR TESTING/DEBUGGING ONLY.
    // GET: api/calendar/scrape-altinget-now
    [HttpGet("scrape-altinget-now")]
    public async Task<ActionResult<List<CalendarEvent>>> TriggerAltingetScrape()
    {
        try
        {
            // Directly call the scraping method
            List<CalendarEvent> events = await _scraperService.ScrapeEventsAsync();

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
}
