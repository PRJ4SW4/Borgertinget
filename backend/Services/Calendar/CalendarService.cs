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
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<CalendarEventDTO>> GetAllEventsAsDTOAsync(int userId)
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
                    InterestedCount = e.InterestedUsers?.Count() ?? 0,
                    IsCurrentUserInterested = e.InterestedUsers?.Any(iu => iu.UserId == userId) ?? false
                })
                .ToList();

            _logger.LogInformation(
                "Successfully fetched and mapped {EventCount} events to DTOs in service.",
                eventDTOs.Count
            );
            return eventDTOs;
        }

        public async Task<(bool IsInterested, int InterestedCount)?> ToggleInterestAsync(int eventId, string userId)
    {
        
        _logger.LogInformation("ToggleInterestAsync kaldt med eventId: {EventId}, userId: {UserId}", eventId, userId);
        if (!int.TryParse(userId, out int parsedUserIdAsInt) || parsedUserIdAsInt == 0) // userId for databasen
        {
            _logger.LogWarning("ToggleInterestAsync: Ugyldigt userIdString format eller værdi: {UserIdString}", userId);
            return null;
        }
        var user = await _calendarEventRepository.GetUserModelByIdStringAsync(userId);
        var calendarEvent = await _calendarEventRepository.GetEventByIdAsync(eventId);
        if (user == null || calendarEvent == null)
        {
            _logger.LogWarning("Event with ID {EventId} or User with ID {UserId} not found.", eventId, userId);
            return null;
        }

        var alreadyInterested = await _calendarEventRepository.RetrieveInterestPairsAsync(eventId, userId);
        bool isNowInterested;

        if (alreadyInterested != null)
        {
            _calendarEventRepository.RemoveEventInterest(alreadyInterested); // Remove interest.
            isNowInterested = false;
        } else {
            var userInterest = new EventInterest
            {
                CalendarEventId = eventId,
                UserId = parsedUserIdAsInt
            };

            _calendarEventRepository.AddEventInterest(userInterest); // Add interest.
            isNowInterested = true;
        }

        try
        {
            await _calendarEventRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save changes after toggling interest.");
            return null;
        }

        int newCount = await _calendarEventRepository.GetInterestedUsersAsync(eventId);

        return (isNowInterested, newCount);
    }

    public async Task<int> GetAmountInterestedAsync(int eventId)
    {
        var interestedUsers = await _calendarEventRepository.GetInterestedUsersAsync(eventId);
        return interestedUsers;
    }
}
}