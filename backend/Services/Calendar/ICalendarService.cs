using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO.Calendar;

// This interface defines a contract for a calendar service that provides methods to interact with calendar events.
namespace backend.Services.Calendar
{
    public interface ICalendarService
    {
        Task<IEnumerable<CalendarEventDTO>> GetAllEventsAsDTOAsync(int userId);

        Task<(bool IsInterested, int InterestedCount)?> ToggleInterestAsync(
            int eventId,
            string userId
        );

        Task<int> GetAmountInterestedAsync(int eventId);
        Task<CalendarEventDTO> CreateEventAsync(CalendarEventDTO calendarEventDto);
        Task<bool> UpdateEventAsync(int id, CalendarEventDTO calendarEventDto);
        Task<bool> DeleteEventAsync(int id);
    }
}
