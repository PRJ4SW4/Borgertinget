using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.Calendar;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using Microsoft.Extensions.Logging;

namespace backend.Services.Calendar
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarEventRepository _calendarEventRepository;
        private readonly ILogger<CalendarService> _logger;

        public CalendarService(
            ICalendarEventRepository calendarEventRepository,
            ILogger<CalendarService> logger
        )
        {
            _calendarEventRepository = calendarEventRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CalendarEventDTO>> GetAllEventsAsDTOAsync()
        {
            _logger.LogInformation("Fetching all calendar events via Calendar Repository.");
            var calendarEvents = await _calendarEventRepository.GetAllEventsAsync();

            if (calendarEvents == null || !calendarEvents.Any())
            {
                _logger.LogWarning("No calendar events found by the repository.");
                return Enumerable.Empty<CalendarEventDTO>();
            }

            var eventDTOs = calendarEvents
                .Select(e => new CalendarEventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTimeUtc = e.StartDateTimeUtc,
                    Location = e.Location,
                    SourceUrl = e.SourceUrl,
                })
                .ToList();

            _logger.LogInformation(
                "Successfully fetched and mapped {EventCount} events to DTOs in service.",
                eventDTOs.Count
            );
            return eventDTOs;
        }
    }
}
