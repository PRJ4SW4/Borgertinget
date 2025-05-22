namespace backend.Services.Calendar.Scraping;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using backend.Services.Calendar.HtmlFetching;
using backend.Services.Calendar.Parsing;
using backend.utils.TimeZone;
using Microsoft.Extensions.Logging;

// This service is responsible for orchestrating the scraping of event data from the Altinget calendar.
public class AltingetScraperService : IScraperService
{
    private readonly IHtmlFetcher _htmlFetcher; // Service for fetching HTML content.
    private readonly IEventDataParser _eventDataParser; // Service for parsing HTML into event data.
    private readonly ICalendarEventRepository _calendarEventRepository; // Repository for calendar event data operations.
    private readonly ILogger<AltingetScraperService> _logger; // Logger for recording service activity and errors.
    private readonly ITimeZoneHelper _timeZoneHelper;
    private const string AltingetCalendarUrl = "https://www.altinget.dk/kalender"; // The URL of the Altinget calendar to scrape.

    // Constructor:  Takes injected dependencies for HTML fetching, parsing, data repository, logging, and timezone helper.
    public AltingetScraperService(
        IHtmlFetcher htmlFetcher,
        IEventDataParser eventDataParser,
        ICalendarEventRepository calendarEventRepository,
        ILogger<AltingetScraperService> logger,
        ITimeZoneHelper timeZoneHelper
    )
    {
        _htmlFetcher = htmlFetcher ?? throw new ArgumentNullException(nameof(htmlFetcher));
        _eventDataParser =
            eventDataParser ?? throw new ArgumentNullException(nameof(eventDataParser));
        _calendarEventRepository =
            calendarEventRepository
            ?? throw new ArgumentNullException(nameof(calendarEventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeZoneHelper = timeZoneHelper ?? throw new ArgumentNullException(nameof(timeZoneHelper));
    }

    // Fetches HTML from Altinget and parses it into a list of ScrapedEventData.
    internal async Task<List<ScrapedEventData>> FetchAndParseAltingetCalendarAsync()
    {
        _logger.LogInformation(
            "Attempting to fetch and parse Altinget calendar data from {Url}",
            AltingetCalendarUrl
        );
        string? htmlContent = await _htmlFetcher.FetchHtmlAsync(AltingetCalendarUrl); // Fetch HTML content.

        if (string.IsNullOrEmpty(htmlContent))
        {
            _logger.LogWarning("HTML content was null or empty. Cannot parse events.");
            return new List<ScrapedEventData>(); // Return an empty list if no HTML content was fetched.
        }

        List<ScrapedEventData> scrapedEvents = _eventDataParser.ParseEvents(htmlContent); // Parse the HTML content.
        _logger.LogInformation(
            "Parsed {EventCount} potential events from HTML content.",
            scrapedEvents.Count
        );
        return scrapedEvents; // Return the list of parsed events.
    }

    // --- RunAutomation with Database Sync & Deletion of Past Events ---
    public async Task<int> RunScraper()
    {
        _logger.LogInformation("Starting Altinget scrape and sync automation..."); // Log the start of the automation process.
        int addedCount = 0; // Counter for newly added events.
        int updatedCount = 0; // Counter for updated events.
        int skippedCount = 0; // Counter for skipped events.
        int deletedPastCount = 0; // Counter for past events deleted.
        int deletedFutureCount = 0; // Counter for future events deleted (those that disappeared from the scrape).

        try
        {
            // --- Step 0: Determine Time Thresholds and Delete Old Events ---
            TimeZoneInfo cetZone = _timeZoneHelper.FindTimeZone();
            DateTimeOffset nowUtcForScrape = DateTimeOffset.UtcNow; // Use DateTimeOffset.UtcNow
            DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtcForScrape, cetZone); // Convert current UTC time to Copenhagen time.
            DateTime startOfTodayCopenhagen = nowCopenhagen.Date; // Get the start of the current day in Copenhagen.
            DateTimeOffset startOfTodayWithOffset = new DateTimeOffset(
                startOfTodayCopenhagen,
                cetZone.GetUtcOffset(startOfTodayCopenhagen)
            ); // Create a DateTimeOffset for the start of today in Copenhagen, with the correct offset.
            DateTimeOffset startOfTodayUtcForQuery = startOfTodayWithOffset.ToUniversalTime(); // Convert the start of today in Copenhagen to UTC for database queries.

            deletedPastCount = await _calendarEventRepository.MarkPastEventsForDeletionAsync(
                startOfTodayUtcForQuery
            ); // Mark past events for deletion.
            // --- End Delete Old Events ---

            // 1. Scrape current events
            List<ScrapedEventData> scrapedEvents = await FetchAndParseAltingetCalendarAsync(); // Fetch and parse events from Altinget.
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
            var existingEventsDict = await _calendarEventRepository.GetFutureEventsBySourceUrlAsync(
                startOfTodayUtcForQuery
            ); // Get existing future events.

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
                    _logger.LogDebug(
                        "Skipping event due to missing SourceUrl, Title, or StartDateTimeUnspecified: {ScrapedEventDetails}",
                        scrapedEvent.SourceUrl ?? "N/A"
                    );
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
                        "Skipping event '{Title}' (SourceUrl: {SourceUrl}) due to DateTime conversion error.",
                        scrapedEvent.Title,
                        scrapedEvent.SourceUrl
                    ); // Log a warning if there is an error converting the DateTime.
                    skippedCount++; // Increment the counter for skipped events.
                    continue; // Skip to the next event.
                }

                // Skip events that are scheduled for before today.
                if (startDateTimeUtc < startOfTodayUtcForQuery)
                {
                    skippedCount++; // Increment the counter for skipped events.
                    _logger.LogDebug(
                        "Skipping past event '{Title}' (SourceUrl: {SourceUrl}) with StartDateTimeUtc {StartDateTimeUtc}",
                        scrapedEvent.Title,
                        scrapedEvent.SourceUrl,
                        startDateTimeUtc
                    );
                    continue; // Skip to the next event.
                }

                // Check if the event already exists in the database.
                if (
                    existingEventsDict.TryGetValue(
                        scrapedEvent.SourceUrl, // SourceUrl is validated not null/whitespace above
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
                        _calendarEventRepository.UpdateEvent(existingEvent); // Stage the update.
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
                        SourceUrl = scrapedEvent.SourceUrl, // SourceUrl is validated not null/whitespace above
                        LastScrapedUtc = DateTimeOffset.UtcNow,
                    };
                    await _calendarEventRepository.AddEventAsync(newEvent); // Stage the addition.
                    addedCount++; // Increment the counter for newly added events.
                    _logger.LogInformation(
                        "Adding new event: Title='{EventTitle}', SourceUrl='{SourceUrl}'",
                        newEvent.Title,
                        newEvent.SourceUrl
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
                _calendarEventRepository.MarkEventsForDeletion(existingEventsDict.Values); // Stage deletions.
                foreach (var eventToDelete in existingEventsDict.Values)
                {
                    _logger.LogInformation(
                        "- ID={EventId}, Title='{EventTitle}' ({SourceUrl})",
                        eventToDelete.Id,
                        eventToDelete.Title,
                        eventToDelete.SourceUrl
                    ); // Log the deletion.
                }
            }

            // 5. Save all changes
            int changes = await _calendarEventRepository.SaveChangesAsync(); // Save all staged changes to the database.
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
            _logger.LogCritical(
                tzEx,
                "CRITICAL ERROR: Copenhagen timezone not found. Automation cannot proceed correctly."
            ); // Log a critical error if the Copenhagen timezone is not found.
            return -1; // Return -1 to indicate an error.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR during Altinget sync automation process."); // Log an error if there is an error during the Altinget sync.
            return -1; // Return -1 to indicate an error.
        }
    }
}
