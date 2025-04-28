// Services/AltingetScraperService.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using backend.Data;
using backend.Models; // Assuming CalendarEvent entity is here
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace backend.Services.AutomationServices
{
    // Temporary class to hold raw scraped data
    public class ScrapedEventData
    {
        public DateOnly? EventDate { get; set; }
        public TimeOnly? EventTime { get; set; }
        public DateTime? StartDateTimeUnspecified { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? SourceUrl { get; set; }
        public string RawDate { get; set; } = string.Empty;
        public string RawTime { get; set; } = string.Empty;
    }

    public class AltingetScraperService : IAutomationService
    {
        private readonly DataContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AltingetScraperService> _logger; // Added Logger
        private const string AltingetCalendarUrl = "https://www.altinget.dk/kalender";
        private const string CustomUserAgent =
            "MyBorgertingetCalendarBot/1.0 (+http://borgertinget/botinfo)";

        public AltingetScraperService(
            DataContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<AltingetScraperService> logger
        ) // Added logger parameter
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClientFactory =
                httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Store logger
        }

        // Scrapes and returns raw data including SourceUrl
        internal async Task<List<ScrapedEventData>> ScrapeEventsAsyncInternal()
        {
            // ... (Scraping logic remains the same as your provided version) ...
            var eventsList = new List<ScrapedEventData>();
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(CustomUserAgent);
            string htmlContent;
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(AltingetCalendarUrl);
                response.EnsureSuccessStatusCode();
                htmlContent = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(
                    e,
                    "Error fetching Altinget calendar page: {ErrorMessage}",
                    e.Message
                );
                return eventsList;
            }
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            string dayGroupXPath =
                "//div[@class='mb-6' and .//div[contains(@class, 'list-title-s')]]";
            string dateXPath =
                ".//div[contains(@class, 'list-title-s') and contains(@class, 'text-red')][1]";
            string eventLinkXPath =
                ".//a[contains(@class, 'bg-white') and contains(@class, 'border-gray-300') and contains(@class, 'block')]";
            string timeXPath = "(.//div[contains(@class, 'list-title-xs')])[1]";
            string titleXPath = "(.//div[contains(@class, 'list-title-xs')])[2]";
            string locationXPath = ".//div[contains(@class, 'list-label')]//span";
            var dayGroupNodes = htmlDocument.DocumentNode.SelectNodes(dayGroupXPath);
            if (dayGroupNodes == null || !dayGroupNodes.Any())
            {
                _logger.LogWarning(
                    "Could not find day group nodes with XPath: {XPath}",
                    dayGroupXPath
                );
                return eventsList;
            }
            DateOnly currentDate = DateOnly.MinValue;
            CultureInfo dkCulture = new CultureInfo("da-DK");
            foreach (var dayGroupNode in dayGroupNodes)
            {
                var dateNode = dayGroupNode.SelectSingleNode(dateXPath);
                string rawDate =
                    dateNode != null ? HtmlEntity.DeEntitize(dateNode.InnerText).Trim() : "";
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
                    currentDate = parsedDate;
                }
                else if (currentDate == DateOnly.MinValue)
                {
                    _logger.LogWarning(
                        "Skipping day group, could not parse initial date: {RawDate}",
                        rawDate
                    );
                    continue;
                }
                var eventLinkNodes = dayGroupNode.SelectNodes(eventLinkXPath);
                if (eventLinkNodes == null)
                    continue;
                foreach (var eventLinkNode in eventLinkNodes)
                {
                    string? sourceUrl = eventLinkNode.Attributes["href"]?.Value;
                    if (string.IsNullOrWhiteSpace(sourceUrl))
                    {
                        _logger.LogWarning("Skipping event - could not find source URL (href).");
                        continue;
                    }
                    var scrapedEvent = new ScrapedEventData
                    {
                        EventDate = currentDate,
                        RawDate = rawDate,
                        SourceUrl = sourceUrl,
                    };
                    var timeNode = eventLinkNode.SelectSingleNode(timeXPath);
                    scrapedEvent.RawTime = timeNode?.InnerText.Trim() ?? "";
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
                        scrapedEvent.EventTime = parsedTime;
                        try
                        {
                            scrapedEvent.StartDateTimeUnspecified = new DateTime(
                                currentDate,
                                parsedTime,
                                DateTimeKind.Unspecified
                            );
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
                            );
                        }
                        catch { }
                    }
                    var titleNode = eventLinkNode.SelectSingleNode(titleXPath);
                    scrapedEvent.Title =
                        titleNode != null
                            ? HtmlEntity.DeEntitize(titleNode.InnerText).Trim()
                            : "Ukendt Titel";
                    var locationNode = eventLinkNode.SelectSingleNode(locationXPath);
                    scrapedEvent.Location =
                        locationNode != null
                            ? HtmlEntity.DeEntitize(locationNode.InnerText).Trim()
                            : null;
                    if (
                        !string.IsNullOrWhiteSpace(scrapedEvent.Title)
                        && scrapedEvent.Title != "Ukendt Titel"
                    )
                    {
                        eventsList.Add(scrapedEvent);
                    }
                }
            }
            _logger.LogInformation("Scraped {EventCount} events.", eventsList.Count);
            return eventsList;
        }

        // --- RunAutomation with Database Sync AND Deletion of Past Events ---
        public async Task<int> RunAutomation()
        {
            _logger.LogInformation("Starting Altinget scrape and sync automation...");
            int addedCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;
            int deletedPastCount = 0;
            int deletedFutureCount = 0;

            try
            {
                // --- Step 0: Delete Old Events ---
                TimeZoneInfo cetZone = FindTimeZone();
                DateTimeOffset nowUtcForScrape = DateTimeOffset.UtcNow; // Use DateTimeOffset.UtcNow
                DateTimeOffset nowCopenhagen = TimeZoneInfo.ConvertTime(nowUtcForScrape, cetZone);
                DateTime startOfTodayCopenhagen = nowCopenhagen.Date;
                DateTimeOffset startOfTodayWithOffset = new DateTimeOffset(
                    startOfTodayCopenhagen,
                    cetZone.GetUtcOffset(startOfTodayCopenhagen)
                );
                DateTimeOffset startOfTodayUtcForQuery = startOfTodayWithOffset.ToUniversalTime();

                var oldEventsToDelete = await this
                    ._context.CalendarEvents.Where(e =>
                        e.StartDateTimeUtc < startOfTodayUtcForQuery
                    )
                    .ToListAsync();

                if (oldEventsToDelete.Any())
                {
                    this._context.CalendarEvents.RemoveRange(oldEventsToDelete);
                    deletedPastCount = oldEventsToDelete.Count;
                    _logger.LogInformation(
                        "Marked {Count} past events (before {ThresholdUtc:O}) for deletion.",
                        deletedPastCount,
                        startOfTodayUtcForQuery
                    );
                }
                // --- End Delete Old Events ---


                // 1. Scrape current events
                List<ScrapedEventData> scrapedEvents = await ScrapeEventsAsyncInternal();
                _logger.LogInformation("Scrape found {Count} events.", scrapedEvents.Count);

                if (!scrapedEvents.Any() && deletedPastCount == 0)
                {
                    _logger.LogInformation(
                        "No events scraped and no past events deleted, sync finished."
                    );
                    return 0;
                }

                // 2. Get existing events from DB for comparison
                var existingEventsDict = await this
                    ._context.CalendarEvents.Where(e =>
                        e.StartDateTimeUtc >= startOfTodayUtcForQuery
                    )
                    .ToDictionaryAsync(e => e.SourceUrl, e => e);

                // 3. Process Scraped Events (Add or Update)
                foreach (var scrapedEvent in scrapedEvents)
                {
                    if (
                        string.IsNullOrWhiteSpace(scrapedEvent.SourceUrl)
                        || string.IsNullOrWhiteSpace(scrapedEvent.Title)
                        || !scrapedEvent.StartDateTimeUnspecified.HasValue
                    )
                    {
                        skippedCount++;
                        continue;
                    }

                    DateTimeOffset startDateTimeUtc;
                    try
                    {
                        DateTime unspecifiedDateTime = scrapedEvent.StartDateTimeUnspecified.Value;
                        startDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(
                            unspecifiedDateTime,
                            cetZone
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Skipping event '{Title}' due to DateTime conversion error.",
                            scrapedEvent.Title
                        );
                        skippedCount++;
                        continue;
                    }

                    if (startDateTimeUtc < startOfTodayUtcForQuery)
                    {
                        skippedCount++;
                        continue;
                    }

                    if (
                        existingEventsDict.TryGetValue(
                            scrapedEvent.SourceUrl,
                            out CalendarEvent? existingEvent
                        )
                    )
                    {
                        // UPDATE check
                        bool updated = false;
                        if (existingEvent.Title != scrapedEvent.Title)
                        {
                            existingEvent.Title = scrapedEvent.Title;
                            updated = true;
                        }
                        if (existingEvent.StartDateTimeUtc != startDateTimeUtc)
                        {
                            existingEvent.StartDateTimeUtc = startDateTimeUtc;
                            updated = true;
                        }
                        if (existingEvent.Location != scrapedEvent.Location)
                        {
                            existingEvent.Location = scrapedEvent.Location;
                            updated = true;
                        }

                        // --- MODIFIED: Use DateTimeOffset.UtcNow ---
                        // IMPORTANT: Assumes CalendarEvent.LastScrapedUtc is now DateTimeOffset
                        existingEvent.LastScrapedUtc = DateTimeOffset.UtcNow;
                        // ---

                        if (updated)
                        {
                            this._context.CalendarEvents.Update(existingEvent);
                            updatedCount++;
                            _logger.LogInformation(
                                "Updating event: ID={EventId}, Title='{EventTitle}'",
                                existingEvent.Id,
                                existingEvent.Title
                            );
                        }
                        existingEventsDict.Remove(scrapedEvent.SourceUrl);
                    }
                    else
                    {
                        // ADD NEW
                        var newEvent = new CalendarEvent
                        {
                            Title = scrapedEvent.Title,
                            StartDateTimeUtc = startDateTimeUtc,
                            Location = scrapedEvent.Location,
                            SourceUrl = scrapedEvent.SourceUrl,
                            // --- MODIFIED: Use DateTimeOffset.UtcNow ---
                            // IMPORTANT: Assumes CalendarEvent.LastScrapedUtc is now DateTimeOffset
                            LastScrapedUtc = DateTimeOffset.UtcNow,
                            // ---
                        };
                        this._context.CalendarEvents.Add(newEvent);
                        addedCount++;
                        _logger.LogInformation(
                            "Adding new event: Title='{EventTitle}'",
                            newEvent.Title
                        );
                    }
                }

                // 4. Handle Deletes for *future* events not found in scrape
                if (existingEventsDict.Any())
                {
                    deletedFutureCount = existingEventsDict.Count;
                    _logger.LogInformation(
                        "Marking {Count} future/current events for deletion (not found in scrape):",
                        deletedFutureCount
                    );
                    foreach (var eventToDelete in existingEventsDict.Values)
                    {
                        _logger.LogInformation(
                            "- ID={EventId}, Title='{EventTitle}' ({SourceUrl})",
                            eventToDelete.Id,
                            eventToDelete.Title,
                            eventToDelete.SourceUrl
                        );
                        this._context.CalendarEvents.Remove(eventToDelete);
                    }
                }

                // 5. Save all changes
                int changes = await this._context.SaveChangesAsync();
                _logger.LogInformation(
                    "Database sync complete. Changes saved: {DbChanges}. Added: {Added}, Updated: {Updated}, DeletedPast: {DeletedPast}, DeletedFuture: {DeletedFuture}, Skipped: {Skipped}.",
                    changes,
                    addedCount,
                    updatedCount,
                    deletedPastCount,
                    deletedFutureCount,
                    skippedCount
                );

                return addedCount + updatedCount + deletedPastCount + deletedFutureCount;
            }
            catch (TimeZoneNotFoundException tzEx)
            {
                _logger.LogCritical(tzEx, "CRITICAL ERROR: Copenhagen timezone not found.");
                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR during Altinget sync.");
                return -1;
            }
        }

        // Helper to find the timezone reliably on different OS
        private TimeZoneInfo FindTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
            }
            catch (TimeZoneNotFoundException) { }
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            }
            catch (TimeZoneNotFoundException ex)
            {
                _logger.LogCritical(
                    ex,
                    "Could not find Copenhagen timezone using either IANA or Windows ID."
                );
                throw;
            }
        }
    }
}
