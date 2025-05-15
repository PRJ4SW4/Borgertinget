namespace backend.Models.Calendar;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class EventInterest
{
    [Key]
    public int EventInterestId {get; set;}

    // Foreign key for CalendarEvent
    public int CalendarEventId { get; set; } 

    [ForeignKey("CalendarEventId")] 
    public virtual CalendarEvent CalendarEvent { get; set; } = null!;

    // Foreign key for User
    public int UserId { get; set; } 

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
