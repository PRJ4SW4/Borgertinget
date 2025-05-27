namespace backend.DTO.Calendar;

public class CalendarEventDTO
{
    public int Id { get; set; } // Event ID from database
    public string Title { get; set; } = string.Empty;

    // Send the full DateTimeOffset string
    // Frontend will handle timezone conversion and formatting for display
    public DateTimeOffset StartDateTimeUtc { get; set; }
    public string? Location { get; set; }
    public string? SourceUrl { get; set; } // Include URL so frontend can link directly to Altinget on click for more details
    public int InterestedCount { get; set; } // Number of users interested in this event
    public bool IsCurrentUserInterested { get; set; } // Whether the current user is interested in this event
}
