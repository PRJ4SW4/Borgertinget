// /backend/Services/AutomationServices/AltingetScraperService.cs
namespace backend.Services.AutomationServices;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using backend.Data;
using backend.Models.Calendar;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// Temporary class to hold raw scraped data
public class ScrapedEventData
{
    // The date of the event, parsed from the scraped data.  Nullable because parsing might fail.
    public DateOnly? EventDate { get; set; }

    // The time of the event, parsed from the scraped data. Nullable because parsing might fail.
    public TimeOnly? EventTime { get; set; }

    // A combined DateTime object, created from EventDate and EventTime.  DateTimeKind is Unspecified.
    // Used for initial processing before timezone conversion. Nullable because parsing might fail.
    public DateTime? StartDateTimeUnspecified { get; set; }

    // The title of the event.  Defaults to an empty string to ensure non-nullability.
    public string Title { get; set; } = string.Empty;

    // The location of the event.  Nullable because it might not always be present.
    public string? Location { get; set; }

    // The URL where the event information was scraped from.  Nullable because it might not always be present.
    public string? SourceUrl { get; set; }

    // The raw date string as scraped from the source.  Useful for debugging and fallback scenarios.
    public string RawDate { get; set; } = string.Empty;

    // The raw time string as scraped from the source.  Useful for debugging and fallback scenarios.
    public string RawTime { get; set; } = string.Empty;
}

// This service is responsible for scraping event data from the Altinget calendar,
// synchronizing it with the database, and deleting past events.
public class AltingetScraperService : IAutomationService
{
    private readonly DataContext _context; // Database context for accessing and modifying data.
    private readonly IHttpClientFactory _httpClientFactory; // Factory for creating HTTP clients.
    private readonly ILogger<AltingetScraperService> _logger; // Logger for recording service activity and errors.
    private const string AltingetCalendarUrl = "https://www.altinget.dk/kalender"; // The URL of the Altinget calendar to scrape.
    private const string CustomUserAgent =
        "MyBorgertingetCalendarBot/1.0 (+http://borgertinget/botinfo)"; // Custom User-Agent string for HTTP requests.

    // Constructor:  Takes injected dependencies for database access, HTTP requests, and logging.
    public AltingetScraperService(
        DataContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<AltingetScraperService> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpClientFactory =
            httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Scrapes and returns raw data including SourceUrl
    internal async Task<List<ScrapedEventData>> ScrapeEventsAsyncInternal()
    {
        var eventsList = new List<ScrapedEventData>(); // Initialize the list to store scraped events.
        var httpClient = _httpClientFactory.CreateClient(); // Create a new HTTP client using the factory.
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(CustomUserAgent); // Set a custom User-Agent header for the HTTP client.
        string htmlContent; // Variable to store the HTML content of the scraped page.
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(AltingetCalendarUrl); // Send an HTTP GET request to the Altinget calendar URL.
            response.EnsureSuccessStatusCode(); // Ensure that the HTTP response status code indicates success.
            htmlContent = await response.Content.ReadAsStringAsync(); // Read the HTML content from the response.
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error fetching Altinget calendar page: {ErrorMessage}", e.Message); // Log an error if the HTTP request fails.
            return eventsList; // Return an empty list if the HTTP request fails.
        }
        var htmlDocument = new HtmlDocument(); // Create a new HTML document object.
        htmlDocument.LoadHtml(htmlContent); // Load the HTML content into the HTML document.

        // Define XPath expressions to locate specific elements in the HTML structure.
        string dayGroupXPath = "//div[@class='mb-6' and .//div[contains(@class, 'list-title-s')]]"; // XPath to find the container for each day's events.
        string dateXPath =
            ".//div[contains(@class, 'list-title-s') and contains(@class, 'text-red')][1]"; // XPath to find the date of the events.
        string eventLinkXPath =
            ".//a[contains(@class, 'bg-white') and contains(@class, 'border-gray-300') and contains(@class, 'block')]"; // XPath to find the link to each event.
        string timeXPath = "(.//div[contains(@class, 'list-title-xs')])[1]"; // XPath to find the time of each event.
        string titleXPath = "(.//div[contains(@class, 'list-title-xs')])[2]"; // XPath to find the title of each event.
        string locationXPath = ".//div[contains(@class, 'list-label')]//span"; // XPath to find the location of each event.

        var dayGroupNodes = htmlDocument.DocumentNode.SelectNodes(dayGroupXPath); // Select all day group nodes using the XPath expression.

        if (dayGroupNodes == null || !dayGroupNodes.Any())
        {
            _logger.LogWarning("Could not find day group nodes with XPath: {XPath}", dayGroupXPath);
            return eventsList; // Return an empty list if no day group nodes are found.
        }

        DateOnly currentDate = DateOnly.MinValue; // Initialize the current date to the minimum possible value.
        CultureInfo dkCulture = new CultureInfo("da-DK"); // Create a CultureInfo object for the Danish (Denmark) culture.

        // Iterate through each day group node.
        foreach (var dayGroupNode in dayGroupNodes)
        {
            string rawDate = "";
            var dateNode = dayGroupNode.SelectSingleNode(dateXPath);
            if (dateNode != null)
            {
                HtmlNode node = dateNode; // assign to non-nullable local
                rawDate = HtmlEntity.DeEntitize(node.InnerText).Trim();
            }

            // Try to parse the raw date string into a DateOnly object.
            if (
                !string.IsNullOrEmpty(rawDate)
                && DateOnly.TryParseExact(
                    rawDate,
                    "d. MMMM yyyy",
                    dkCulture,
                    DateTimeStyles.None,
                    out DateOnly parsedDate
                )
            )
            {
                currentDate = parsedDate; // Update the current date if parsing is successful.
            }
            else if (currentDate == DateOnly.MinValue)
            {
                _logger.LogWarning(
                    "Skipping day group, could not parse initial date: {RawDate}",
                    rawDate
                );
                continue; // Skip the current day group if the date cannot be parsed and no date has been parsed yet.
            }

            var eventLinkNodes = dayGroupNode.SelectNodes(eventLinkXPath); // Select all event link nodes within the current day group node.
            if (eventLinkNodes == null)
                continue; // Skip to the next day group if no event link nodes are found.

            // Iterate through each event link node.
            foreach (var eventLinkNode in eventLinkNodes)
            {
                string? sourceUrl = eventLinkNode.Attributes["href"]?.Value; // Extract the source URL from the event link node.
                if (string.IsNullOrWhiteSpace(sourceUrl))
                {
                    _logger.LogWarning("Skipping event - could not find source URL (href).");
                    continue; // Skip the current event if the source URL is missing.
                }

                var scrapedEvent = new ScrapedEventData
                {
                    // Create a new ScrapedEventData object and populate it with the scraped data.
                    EventDate = currentDate,
                    RawDate = rawDate,
                    SourceUrl = sourceUrl,
                };

                var timeNode = eventLinkNode.SelectSingleNode(timeXPath); // Select the time node within the current event link node.
                scrapedEvent.RawTime = timeNode?.InnerText.Trim() ?? ""; // Extract the raw time string from the time node.

                // Try to parse the raw time string into a TimeOnly object.
                if (
                    TimeOnly.TryParseExact(
                        scrapedEvent.RawTime,
                        "HH.mm",
                        dkCulture,
                        DateTimeStyles.None,
                        out TimeOnly parsedTime
                    )
                )
                {
                    scrapedEvent.EventTime = parsedTime; // Update the event time if parsing is successful.
                    try
                    {
                        scrapedEvent.StartDateTimeUnspecified = new DateTime(
                            currentDate,
                            parsedTime,
                            DateTimeKind.Unspecified
                        ); // Combine the date and time into a DateTime object (DateTimeKind is Unspecified).
                    }
                    catch { }
                }
                else if (scrapedEvent.RawTime == "00.00")
                {
                    try
                    {
                        scrapedEvent.StartDateTimeUnspecified = new DateTime(
                            currentDate,
                            TimeOnly.MinValue,
                            DateTimeKind.Unspecified
                        ); // If the time is "00.00", set the time to the minimum possible value.
                    }
                    catch { }
                }

                var titleNode = eventLinkNode.SelectSingleNode(titleXPath); // Select the title node within the current event link node.
                scrapedEvent.Title =
                    titleNode != null
                        ? HtmlEntity.DeEntitize(titleNode.InnerText).Trim()
                        : "Ukendt Titel"; // Extract the title from the title node.

                var locationNode = eventLinkNode.SelectSingleNode(locationXPath); // Select the location node within the current event link node.
                scrapedEvent.Location =
                    locationNode != null
                        ? HtmlEntity.DeEntitize(locationNode.InnerText).Trim()
                        : null; // Extract the location from the location node.

                // Add the scraped event to the list if the title is not empty or "Ukendt Titel".
                if (
                    !string.IsNullOrWhiteSpace(scrapedEvent.Title)
                    && scrapedEvent.Title != "Ukendt Titel"
                )
                {
                    eventsList.Add(scrapedEvent);
                }
            }
        }
        _logger.LogInformation("Scraped {EventCount} events.", eventsList.Count); // Log the number of scraped events.
        return eventsList; // Return the list of scraped events.
    }

    // --- RunAutomation with Database Sync & Deletion of Past Events ---
    public async Task<int> RunAutomation()
    {
        _logger.LogInformation("Starting Altinget scrape and sync automation..."); // Log the start of the automation process.
        int addedCount = 0; // Counter for newly added events.
        int updatedCount = 0; // Counter for updated events.
        int skippedCount = 0; // Counter for skipped events.
        int deletedPastCount = 0; // Counter for past events deleted.
        int deletedFutureCount = 0; // Counter for future events deleted (those that disappeared from the scrape).

        try
        {
            // --- Step 0: Delete Old Events ---
            TimeZoneInfo cetZone = FindTimeZone(); // Get the Central European Time (CET) zone.
            DateTimeOffset nowUtcForScrape = DateTimeOffset.UtcNow; // Use DateTimeOffset.UtcNow
            DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtcForScrape, cetZone); // Convert current UTC time to Copenhagen time.
            DateTime startOfTodayCopenhagen = nowCopenhagen.Date; // Get the start of the current day in Copenhagen.
            DateTimeOffset startOfTodayWithOffset = new DateTimeOffset(
                startOfTodayCopenhagen,
                cetZone.GetUtcOffset(startOfTodayCopenhagen)
            ); // Create a DateTimeOffset for the start of today in Copenhagen, with the correct offset.
            DateTimeOffset startOfTodayUtcForQuery = startOfTodayWithOffset.ToUniversalTime(); // Convert the start of today in Copenhagen to UTC for database queries.

            // Retrieve events from the database that are older than the start of today in UTC.
            var oldEventsToDelete = await this
                ._context.CalendarEvents.Where(e => e.StartDateTimeUtc < startOfTodayUtcForQuery)
                .ToListAsync();

            // If there are old events, remove them from the database.
            if (oldEventsToDelete.Any())
            {
                this._context.CalendarEvents.RemoveRange(oldEventsToDelete); // Remove the old events from the context.
                deletedPastCount = oldEventsToDelete.Count; // Update the counter for deleted past events.
                _logger.LogInformation(
                    "Marked {Count} past events (before {ThresholdUtc:O}) for deletion.",
                    deletedPastCount,
                    startOfTodayUtcForQuery
                ); // Log the number of past events marked for deletion.
            }
            // --- End Delete Old Events ---


            // 1. Scrape current events
            List<ScrapedEventData> scrapedEvents = await ScrapeEventsAsyncInternal(); // Scrape events from Altinget.
            _logger.LogInformation("Scrape found {Count} events.", scrapedEvents.Count); // Log the number of scraped events.

            // If no events are scraped and no past events were deleted, the sync is finished.
            if (!scrapedEvents.Any() && deletedPastCount == 0)
            {
                _logger.LogInformation(
                    "No events scraped and no past events deleted, sync finished."
                );
                return 0; // Return 0 to indicate that no changes were made.
            }

            // 2. Get existing events from DB for comparison
            // Retrieve events from the database that are scheduled for today or later.
            var existingEventsDict = await this
                ._context.CalendarEvents.Where(e => e.StartDateTimeUtc >= startOfTodayUtcForQuery)
                .ToDictionaryAsync(e => e.SourceUrl, e => e); // Convert the events to a dictionary for efficient lookup by SourceUrl.

            // 3. Process Scraped Events (Add or Update)
            // Iterate through the scraped events to add new events or update existing ones.
            foreach (var scrapedEvent in scrapedEvents)
            {
                // Skip events with missing SourceUrl, Title, or StartDateTimeUnspecified.
                if (
                    string.IsNullOrWhiteSpace(scrapedEvent.SourceUrl)
                    || string.IsNullOrWhiteSpace(scrapedEvent.Title)
                    || !scrapedEvent.StartDateTimeUnspecified.HasValue
                )
                {
                    skippedCount++; // Increment the counter for skipped events.
                    continue; // Skip to the next event.
                }

                DateTimeOffset startDateTimeUtc; // Variable to store the event's start date and time in UTC.
                try
                {
                    DateTime unspecifiedDateTime = scrapedEvent.StartDateTimeUnspecified.Value; // Get the unspecified DateTime from the scraped event.
                    startDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(unspecifiedDateTime, cetZone); // Convert the event's start date and time to UTC, using the CET zone.
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Skipping event '{Title}' due to DateTime conversion error.",
                        scrapedEvent.Title
                    ); // Log a warning if there is an error converting the DateTime.
                    skippedCount++; // Increment the counter for skipped events.
                    continue; // Skip to the next event.
                }

                // Skip events that are scheduled for before today.
                if (startDateTimeUtc < startOfTodayUtcForQuery)
                {
                    skippedCount++; // Increment the counter for skipped events.
                    continue; // Skip to the next event.
                }

                // Check if the event already exists in the database.
                if (
                    existingEventsDict.TryGetValue(
                        scrapedEvent.SourceUrl,
                        out CalendarEvent? existingEvent
                    )
                )
                {
                    // UPDATE check
                    bool updated = false; // Flag to indicate whether the event has been updated.

                    // Update the event's title if it has changed.
                    if (existingEvent.Title != scrapedEvent.Title)
                    {
                        existingEvent.Title = scrapedEvent.Title;
                        updated = true; // Set the flag to true.
                    }
                    // Update the event's start date and time if it has changed.
                    if (existingEvent.StartDateTimeUtc != startDateTimeUtc)
                    {
                        existingEvent.StartDateTimeUtc = startDateTimeUtc;
                        updated = true; // Set the flag to true.
                    }
                    // Update the event's location if it has changed.
                    if (existingEvent.Location != scrapedEvent.Location)
                    {
                        existingEvent.Location = scrapedEvent.Location;
                        updated = true; // Set the flag to true.
                    }

                    existingEvent.LastScrapedUtc = DateTimeOffset.UtcNow; // Update the LastScrapedUtc property.

                    // If the event has been updated, update it in the database.
                    if (updated)
                    {
                        this._context.CalendarEvents.Update(existingEvent); // Update the event in the context.
                        updatedCount++; // Increment the counter for updated events.
                        _logger.LogInformation(
                            "Updating event: ID={EventId}, Title='{EventTitle}'",
                            existingEvent.Id,
                            existingEvent.Title
                        ); // Log the update.
                    }
                    existingEventsDict.Remove(scrapedEvent.SourceUrl); // Remove the event from the dictionary of existing events, so it is not considered for deletion.
                }
                else
                {
                    // ADD NEW
                    var newEvent = new CalendarEvent
                    {
                        // Create a new CalendarEvent object and populate it with the scraped data.
                        Title = scrapedEvent.Title,
                        StartDateTimeUtc = startDateTimeUtc,
                        Location = scrapedEvent.Location,
                        SourceUrl = scrapedEvent.SourceUrl,
                        LastScrapedUtc = DateTimeOffset.UtcNow,
                    };
                    this._context.CalendarEvents.Add(newEvent); // Add the new event to the context.
                    addedCount++; // Increment the counter for newly added events.
                    _logger.LogInformation(
                        "Adding new event: Title='{EventTitle}'",
                        newEvent.Title
                    ); // Log the addition.
                }
            }

            // 4. Handle Deletes for *future* events not found in scrape
            // Iterate through the remaining events in the dictionary of existing events.  These are events that
            // were in the database, were scheduled for today or later, but were not found in the scrape.  This means
            // they have been removed from the source calendar, and should be removed from our database as well.
            if (existingEventsDict.Any())
            {
                deletedFutureCount = existingEventsDict.Count; // Set the counter for deleted future events.
                _logger.LogInformation(
                    "Marking {Count} future/current events for deletion (not found in scrape):",
                    deletedFutureCount
                ); // Log the number of future events marked for deletion.
                foreach (var eventToDelete in existingEventsDict.Values)
                {
                    _logger.LogInformation(
                        "- ID={EventId}, Title='{EventTitle}' ({SourceUrl})",
                        eventToDelete.Id,
                        eventToDelete.Title,
                        eventToDelete.SourceUrl
                    ); // Log the deletion.
                    this._context.CalendarEvents.Remove(eventToDelete); // Remove the event from the context.
                }
            }

            // 5. Save all changes
            int changes = await this._context.SaveChangesAsync(); // Save all changes to the database.
            _logger.LogInformation(
                "Database sync complete. Changes saved: {DbChanges}. Added: {Added}, Updated: {Updated}, DeletedPast: {DeletedPast}, DeletedFuture: {DeletedFuture}, Skipped: {Skipped}.",
                changes,
                addedCount,
                updatedCount,
                deletedPastCount,
                deletedFutureCount,
                skippedCount
            ); // Log the completion of the database sync.

            return addedCount + updatedCount + deletedPastCount + deletedFutureCount; // Return the total number of changes.
        }
        catch (TimeZoneNotFoundException tzEx)
        {
            _logger.LogCritical(tzEx, "CRITICAL ERROR: Copenhagen timezone not found."); // Log a critical error if the Copenhagen timezone is not found.
            return -1; // Return -1 to indicate an error.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR during Altinget sync."); // Log an error if there is an error during the Altinget sync.
            return -1; // Return -1 to indicate an error.
        }
    }

    // Helper to find the timezone reliably on different OS
    private TimeZoneInfo FindTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen"); // IANA ID (Linux/macOS)
        }
        catch (TimeZoneNotFoundException) { } // Ignore and try Windows ID
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); // Windows ID
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogCritical(
                ex,
                "Could not find Copenhagen timezone using either IANA or Windows ID."
            ); // Log a critical error if the Copenhagen timezone is not found using either IANA or Windows ID.
            throw; // Re-throw if neither is found
        }
    }
}
