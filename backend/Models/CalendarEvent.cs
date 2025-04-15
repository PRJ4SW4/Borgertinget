// Models/CalendarEvent.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CalendarEvent
{
    [Key] // Primary Key for your database table
    public int Id { get; set; }

    [Required]
    [StringLength(350)] // Allow slightly longer titles
    public string Title { get; set; } = string.Empty;

    [Required]
    // Store the exact point in time, converted to UTC
    public DateTimeOffset StartDateTimeUtc { get; set; }

    [StringLength(300)] // Allow longer locations
    public string? Location { get; set; }

    [Required] // Make this required for reliable matching
    [StringLength(512)] // Store the relative URL from Altinget (e.g., /kalender/57556)
    public string SourceUrl { get; set; } = string.Empty;

    // Track when the scraper last verified this event's data
    public DateTimeOffset LastScrapedUtc { get; set; }
}
