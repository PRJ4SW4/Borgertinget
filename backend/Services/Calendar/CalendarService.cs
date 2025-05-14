using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.Calendar;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using backend.Data;

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

        public async Task<bool> ToggleInterestAsync(int eventId, string userId)
    {
        var user = _calendarEventRepository.GetUserModelByIdStringAsync(userId);
        
        if (user == null || _calendarEventRepository.GetEventByIdAsync(eventId) == null)
        {
            _logger.LogWarning("Event with ID {EventId} or User with ID {UserId} not found.", eventId, userId);
            return false;
        }

        var alreadyInterested = await _calendarEventRepository.RetrieveInterestPairsAsync(eventId, userId);

        if (alreadyInterested != null)
        {
            _calendarEventRepository.RemoveEventInterest(alreadyInterested); // Remove interest.
            await _calendarEventRepository.SaveChangesAsync();
            return true;

        } else {
            var userInterest = new EventInterest
            {
                CalendarEventId = eventId,
                UserId = user.Id
            };

            _calendarEventRepository.AddEventInterest(userInterest); // Add interest.
            await _calendarEventRepository.SaveChangesAsync();
            return true;
        }
    }

    public async Task<int> GetAmountInterestedAsync(int eventId)
    {
        var interestedUsers = await _calendarEventRepository.GetInterestedUsersAsync(eventId);
        return interestedUsers;
    }
}
}