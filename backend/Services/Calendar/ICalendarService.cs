using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO.Calendar;

// This interface defines a contract for a calendar service that provides methods to interact with calendar events.
namespace backend.Services.Calendar
{
    public interface ICalendarService
    {
        Task<IEnumerable<CalendarEventDTO>> GetAllEventsAsDTOAsync();
    }
}
