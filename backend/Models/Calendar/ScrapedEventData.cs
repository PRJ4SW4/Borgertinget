namespace backend.Models.Calendar;

using System;

// This class is used to store the scraped event data before it is processed and saved to the database.
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
