namespace backend.Services.Calendar.Parsing;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using backend.Models.Calendar;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

// Parses HTML content from Altinget's calendar page to extract event data.
public class AltingetEventDataParser : IEventDataParser
{
    private readonly ILogger<AltingetEventDataParser> _logger; // Logger for recording parsing activity and errors.
    private readonly CultureInfo _dkCulture = new("da-DK"); // CultureInfo for Danish (Denmark) specific parsing.

    // XPath expressions to locate specific elements in the Altinget HTML structure.
    // You can get these expressions by inspecting the HTML structure of the Altinget calendar page using Chromium DevTools
    // You can actually just right click the element in DevTools and select "Copy XPath" to get the XPath expression,
    // which would have saved me alot of time had i known at the start ;(
    private const string DayGroupXPath =
        "//div[@class='mb-6' and .//div[contains(@class, 'list-title-s')]]";
    private const string DateXPath =
        ".//div[contains(@class, 'list-title-s') and contains(@class, 'text-red')][1]";
    private const string EventLinkXPath =
        ".//a[contains(@class, 'bg-white') and contains(@class, 'border-gray-300') and contains(@class, 'block')]";
    private const string TimeXPath = "(.//div[contains(@class, 'list-title-xs')])[1]";
    private const string TitleXPath = "(.//div[contains(@class, 'list-title-xs')])[2]";
    private const string LocationXPath = ".//div[contains(@class, 'list-label')]//span";

    // Constructor
    public AltingetEventDataParser(ILogger<AltingetEventDataParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Parses the provided HTML content and extracts event data.
    public List<ScrapedEventData> ParseEvents(string htmlContent)
    {
        var eventsList = new List<ScrapedEventData>(); // Initialize the list to store parsed events.
        if (string.IsNullOrEmpty(htmlContent))
        {
            _logger.LogWarning(
                "HTML content provided for parsing is null or empty. Returning empty list."
            );
            return eventsList;
        }

        var htmlDocument = new HtmlDocument(); // Create a new HTML document object.
        htmlDocument.LoadHtml(htmlContent); // Load the HTML content.

        var dayGroupNodes = htmlDocument.DocumentNode.SelectNodes(DayGroupXPath); // Select all day group nodes.

        if (dayGroupNodes == null || !dayGroupNodes.Any())
        {
            _logger.LogWarning(
                "Could not find day group nodes using XPath: {XPath}. No events will be parsed.",
                DayGroupXPath
            );
            return eventsList; // Return an empty list if no day group nodes are found.
        }

        DateOnly currentDate = DateOnly.MinValue; // Initialize the current date.

        // Iterate through each day group node.
        foreach (var dayGroupNode in dayGroupNodes)
        {
            var dateNode = dayGroupNode.SelectSingleNode(DateXPath); // Select the date node.
            string rawDate =
                dateNode != null && dateNode.InnerText != null
                    ? (HtmlEntity.DeEntitize(dateNode.InnerText)?.Trim() ?? "")
                    : ""; // Extract the raw date string.

            // Try to parse the raw date string.
            if (
                !string.IsNullOrEmpty(rawDate)
                && DateOnly.TryParseExact(
                    rawDate,
                    "d. MMMM yyyy",
                    _dkCulture,
                    DateTimeStyles.None,
                    out DateOnly parsedDate
                )
            )
            {
                currentDate = parsedDate; // Update the current date.
            }
            else if (currentDate == DateOnly.MinValue)
            {
                _logger.LogWarning(
                    "Skipping day group, could not parse initial date from raw string: '{RawDateString}'",
                    rawDate
                );
                continue; // Skip if the date cannot be parsed and no valid date has been set yet.
            }

            var eventLinkNodes = dayGroupNode.SelectNodes(EventLinkXPath); // Select all event link nodes.
            if (eventLinkNodes == null)
                continue; // Skip if no event links are found in this day group.

            // Iterate through each event link node.
            foreach (var eventLinkNode in eventLinkNodes)
            {
                string? sourceUrl = eventLinkNode.Attributes["href"]?.Value; // Extract the source URL.
                if (string.IsNullOrWhiteSpace(sourceUrl))
                {
                    _logger.LogWarning(
                        "Skipping an event entry because its source URL (href attribute) is missing or empty."
                    );
                    continue; // Skip if the source URL is missing.
                }

                var scrapedEvent = new ScrapedEventData // Create a new ScrapedEventData object.
                {
                    EventDate = currentDate,
                    RawDate = rawDate,
                    SourceUrl = sourceUrl,
                };

                var timeNode = eventLinkNode.SelectSingleNode(TimeXPath); // Select the time node.
                scrapedEvent.RawTime = timeNode?.InnerText?.Trim() ?? ""; // Extract the raw time string.

                // Try to parse the raw time string.
                if (
                    TimeOnly.TryParseExact(
                        scrapedEvent.RawTime,
                        "HH.mm",
                        _dkCulture,
                        DateTimeStyles.None,
                        out TimeOnly parsedTime
                    )
                )
                {
                    scrapedEvent.EventTime = parsedTime; // Update the event time.
                    try
                    {
                        scrapedEvent.StartDateTimeUnspecified = new DateTime(
                            currentDate,
                            parsedTime,
                            DateTimeKind.Unspecified
                        ); // Combine date and time.
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Could not create DateTime from Date {Date} and Time {Time} for SourceUrl {SourceUrl}. RawTime: {RawTime}",
                            currentDate,
                            parsedTime,
                            sourceUrl,
                            scrapedEvent.RawTime
                        );
                    }
                }
                else if (scrapedEvent.RawTime == "00.00") // Handle "00.00" as a special case, representing start of the day.
                {
                    scrapedEvent.EventTime = TimeOnly.MinValue;
                    try
                    {
                        scrapedEvent.StartDateTimeUnspecified = new DateTime(
                            currentDate,
                            TimeOnly.MinValue,
                            DateTimeKind.Unspecified
                        );
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Could not create DateTime from Date {Date} and Time 00.00 for SourceUrl {SourceUrl}.",
                            currentDate,
                            sourceUrl
                        );
                    }
                }
                else
                {
                    _logger.LogDebug(
                        "Time '{RawTime}' for event '{SourceUrl}' could not be parsed to HH.mm format.",
                        scrapedEvent.RawTime,
                        scrapedEvent.SourceUrl
                    );
                }

                var titleNode = eventLinkNode.SelectSingleNode(TitleXPath); // Select the title node.
                if (titleNode != null)
                {
                    // InnerText is guaranteed non-null if titleNode is non-null.
                    string titleInnerText = titleNode.InnerText;
                    // DeEntitize can return null if titleInnerText is null, though InnerText itself won't be null here.
                    string? deEntitizedTitle = HtmlEntity.DeEntitize(titleInnerText);
                    scrapedEvent.Title = deEntitizedTitle?.Trim() ?? "Ukendt Titel";
                }
                else
                {
                    scrapedEvent.Title = "Ukendt Titel"; // Extract the title.
                }

                var locationNode = eventLinkNode.SelectSingleNode(LocationXPath); // Select the location node.
                if (locationNode != null)
                {
                    // InnerText is guaranteed non-null if locationNode is non-null.
                    string locationInnerText = locationNode.InnerText;
                    // DeEntitize can return null if locationInnerText is null, though InnerText itself won't be null here.
                    string? deEntitizedLocation = HtmlEntity.DeEntitize(locationInnerText);
                    scrapedEvent.Location = deEntitizedLocation?.Trim();
                }
                else
                {
                    scrapedEvent.Location = null; // Extract the location.
                }

                // Add the event if essential data (Title and a parsable StartDateTime) is present.
                if (
                    !string.IsNullOrWhiteSpace(scrapedEvent.Title)
                    && scrapedEvent.Title != "Ukendt Titel"
                    && scrapedEvent.StartDateTimeUnspecified.HasValue
                )
                {
                    eventsList.Add(scrapedEvent);
                }
                else
                {
                    _logger.LogWarning(
                        "Skipping event with SourceUrl '{SourceUrl}' due to missing critical data. Title: '{Title}', ParsedDateTime: {ParsedDateTime}",
                        scrapedEvent.SourceUrl,
                        scrapedEvent.Title,
                        scrapedEvent.StartDateTimeUnspecified
                    );
                }
            }
        }
        _logger.LogInformation(
            "HTML parsing completed. Extracted {EventCount} valid event data items.",
            eventsList.Count
        );
        return eventsList; // Return the list of parsed events.
    }
}
