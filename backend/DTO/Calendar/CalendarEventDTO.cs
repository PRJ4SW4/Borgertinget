// /backend/DTO/Calendar/CalendarEventDTO.cs
namespace backend.DTO.Calendar;

using System.ComponentModel.DataAnnotations;

public class CalendarEventDTO
{
    public int Id { get; set; } // Event ID from database
    public string Title { get; set; } = string.Empty;

    // Send the full DateTimeOffset string (ISO 8601 format)
    // Frontend will handle timezone conversion and formatting for display
    public DateTimeOffset StartDateTimeUtc { get; set; }
    public string? Location { get; set; }
    public string? SourceUrl { get; set; } // Include URL so frontend can link directly to Altinget on click for more details
}
