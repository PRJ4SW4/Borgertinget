// Models/ScrapedAltingetEvent.cs (or similar location)
public class ScrapedAltingetEvent
{
    public DateOnly? EventDate { get; set; } // Use DateOnly for just the date part
    public TimeOnly? EventTime { get; set; } // Use TimeOnly for just the time part
    public DateTime? StartDateTimeCEST { get; set; } // Combined
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string RawDate { get; set; } = string.Empty; // Store original strings for debugging
    public string RawTime { get; set; } = string.Empty;
}
