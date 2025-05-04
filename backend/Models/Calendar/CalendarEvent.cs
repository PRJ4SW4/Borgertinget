// /backend/Models/Calendar/CalendarEvent.cs
namespace backend.Models.Calendar;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CalendarEvent
{
    [Key] // Primary Key for database table
    public int Id { get; set; }

    [Required]
    [StringLength(350)]
    public string Title { get; set; } = string.Empty;

    [Required]
    // Store the exact point in time, converted to UTC
    public DateTimeOffset StartDateTimeUtc { get; set; }

    [StringLength(300)]
    public string? Location { get; set; }

    [Required] // Has to be required, as this is what allows for proper updating and matching/syncing
    [StringLength(512)] // Store the relative URL from Altinget (e.g., /kalender/57556) to allow for navigating to altinget from our website & syncing
    public string SourceUrl { get; set; } = string.Empty;

    // Track when the scraper last verified this event's data
    public DateTimeOffset LastScrapedUtc { get; set; }
}
