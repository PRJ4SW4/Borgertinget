// Services/AltingetScraperService.cs (New File)
using System;
using System.Collections.Generic;
using System.Globalization; // For parsing Danish dates/times
using System.Net.Http;
using System.Threading.Tasks;
using backend.Data;
using HtmlAgilityPack;
using Newtonsoft.Json; // Use the parser library

namespace backend.Services.AutomationServices;

public class AltingetScraperService : IAutomationService
{
    private readonly DataContext? _context; // Keep if needed for saving later
    private readonly IHttpClientFactory _httpClientFactory; // Inject factory
    private const string AltingetCalendarUrl = "https://www.altinget.dk/kalender";
    private const string CustomUserAgent =
        "MyBorgertingetCalendarBot/1.0 (+http://borgertinget/botinfo)"; // Replace

    // Inject both dependencies
    public AltingetScraperService(DataContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // This method scrapes and returns the list - saving is separate
    public async Task<List<ScrapedAltingetEvent>> ScrapeEventsAsync()
    {
        var eventsList = new List<ScrapedAltingetEvent>();
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
            Console.WriteLine($"Error fetching Altinget calendar page: {e.Message}");
            // Consider proper logging
            return eventsList; // Return empty list
        }

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        // --- Define XPaths (outside loop) ---
        string dayGroupXPath = "//div[@class='mb-6' and .//div[contains(@class, 'list-title-s')]]";
        string dateXPath =
            ".//div[contains(@class, 'list-title-s') and contains(@class, 'text-red')][1]";
        string eventLinkXPath =
            ".//a[contains(@class, 'bg-white') and contains(@class, 'border-gray-300') and contains(@class, 'block')]";
        // Select elements, not text nodes directly
        string timeXPath = "(.//div[contains(@class, 'list-title-xs')])[1]";
        string titleXPath = "(.//div[contains(@class, 'list-title-xs')])[2]";
        string locationXPath = ".//div[contains(@class, 'list-label')]//span"; // Select span element

        var dayGroupNodes = htmlDocument.DocumentNode.SelectNodes(dayGroupXPath);

        if (dayGroupNodes == null || !dayGroupNodes.Any()) // Use Linq's Any() for clarity
        {
            Console.WriteLine("Could not find day group nodes with XPath: " + dayGroupXPath);
            return eventsList;
        }

        DateOnly currentDate = DateOnly.MinValue;
        CultureInfo dkCulture = new CultureInfo("da-DK"); // Use Danish culture for parsing

        foreach (var dayGroupNode in dayGroupNodes)
        {
            var dateNode = dayGroupNode.SelectSingleNode(dateXPath);
            string rawDate =
                dateNode != null ? HtmlEntity.DeEntitize(dateNode.InnerText).Trim() : "";

            // Attempt to parse the date for the current group
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
                // Skip if we couldn't parse the very first date encountered
                Console.WriteLine($"Skipping day group, could not parse initial date: {rawDate}");
                continue;
            }
            // If date parsing fails for subsequent groups, we continue using the previously parsed date

            var eventLinkNodes = dayGroupNode.SelectNodes(eventLinkXPath);
            if (eventLinkNodes == null)
                continue; // No events in this day group

            foreach (var eventLinkNode in eventLinkNodes)
            {
                var scrapedEvent = new ScrapedAltingetEvent
                {
                    EventDate = currentDate,
                    RawDate = rawDate, // Store the raw date string found for this group
                };

                // --- Extract data using SelectSingleNode and checking for null ---
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
                    // Combine Date and Time
                    try
                    {
                        // Store as local time first, conversion to UTC should happen based on timezone rules
                        scrapedEvent.StartDateTimeCEST = new DateTime(
                            currentDate,
                            parsedTime,
                            DateTimeKind.Unspecified
                        );
                        // TODO: Convert to UTC properly using TimeZoneInfo for 'Europe/Copenhagen'
                    }
                    catch { } // Ignore ArgumentOutOfRangeException
                }
                else if (scrapedEvent.RawTime == "00.00")
                {
                    // Handle 00:00 as start of day / all day maybe?
                    try
                    {
                        scrapedEvent.StartDateTimeCEST = new DateTime(
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
        Console.WriteLine($"Scraped {eventsList.Count} events."); // Add some logging
        return eventsList;
    }

    // --- Implement RunAutomation for testing/logging ---
    public async Task<int> RunAutomation() // Make it async Task<int>
    {
        Console.WriteLine(
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting Altinget scrape automation..."
        );
        List<ScrapedAltingetEvent> events;
        try
        {
            // Call the actual scraping method
            events = await ScrapeEventsAsync();
            Console.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Scrape successful. Found {events.Count} events."
            );

            // --- Log scraped data to console using Newtonsoft.Json ---
            // Ensure you have 'using Newtonsoft.Json;' at the top of the file
            string jsonOutput = JsonConvert.SerializeObject(events, Formatting.Indented);
            Console.WriteLine("--- Scraped Events Output (JSON) ---");
            Console.WriteLine(jsonOutput);
            Console.WriteLine("--- End Scraped Events Output ---");
            // --- End logging ---

            // TODO: Implement logic here later to save 'events' to the database using _context
            // For now, just return the count of scraped events
            return events.Count;
        }
        catch (Exception ex)
        {
            // Log any errors during the process
            Console.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR during Altinget scrape automation: {ex.Message}"
            );
            // Console.WriteLine(ex.ToString()); // For more detailed stack trace if needed
            return -1; // Return -1 or throw exception to indicate failure
        }
    }
}
