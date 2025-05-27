using backend.DTO.Calendar;
using backend.Models;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Calendar
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarEventRepository _calendarEventRepository;
        private readonly ILogger<CalendarService> _logger;
        private readonly UserManager<User> _userManager;

        public CalendarService(
            ICalendarEventRepository calendarEventRepository,
            ILogger<CalendarService> logger,
            UserManager<User> userManager
        )
        {
            _calendarEventRepository = calendarEventRepository;
            _logger = logger;
            _userManager = userManager;
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

            var eventDTOs = MapCalendarEventToDTO(calendarEvents, userId);

            _logger.LogInformation(
                "Successfully fetched and mapped {EventCount} events to DTOs in service.",
                eventDTOs.Count
            );
            return eventDTOs;
        }

        private List<CalendarEventDTO> MapCalendarEventToDTO(
            IEnumerable<CalendarEvent> calendarEvent,
            int userId
        )
        {
            return calendarEvent
                .Select(e => new CalendarEventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTimeUtc = e.StartDateTimeUtc,
                    Location = e.Location,
                    SourceUrl = e.SourceUrl,
                    InterestedCount = e.InterestedUsers?.Count() ?? 0,
                    IsCurrentUserInterested =
                        e.InterestedUsers?.Any(iu => iu.UserId == userId) ?? false,
                })
                .ToList();
        }

        public async Task<CalendarEventDTO> CreateEventAsync(CalendarEventDTO calendarEventDto)
        {
            _logger.LogInformation("Creating a new calendar event via Calendar Repository.");
            var calendarEvent = new CalendarEvent
            {
                Title = calendarEventDto.Title,
                StartDateTimeUtc = calendarEventDto.StartDateTimeUtc,
                Location = calendarEventDto.Location,
                SourceUrl = calendarEventDto.SourceUrl ?? string.Empty,
            };

            await _calendarEventRepository.AddEventAsync(calendarEvent);
            await _calendarEventRepository.SaveChangesAsync();

            return new CalendarEventDTO
            {
                Id = calendarEvent.Id,
                Title = calendarEvent.Title,
                StartDateTimeUtc = calendarEvent.StartDateTimeUtc,
                Location = calendarEvent.Location,
                SourceUrl = calendarEvent.SourceUrl,
            };
        }

        public async Task<bool> UpdateEventAsync(int id, CalendarEventDTO calendarEventDto)
        {
            _logger.LogInformation(
                $"Updating calendar event with ID: {id} via Calendar Repository."
            );
            var calendarEvent = await _calendarEventRepository.GetEventByIdAsync(id);

            if (calendarEvent == null)
            {
                _logger.LogWarning($"Calendar event with ID: {id} not found.");
                return false;
            }

            calendarEvent.Title = calendarEventDto.Title;
            calendarEvent.StartDateTimeUtc = calendarEventDto.StartDateTimeUtc;
            calendarEvent.Location = calendarEventDto.Location;
            calendarEvent.SourceUrl = calendarEventDto.SourceUrl ?? string.Empty;

            _calendarEventRepository.UpdateEvent(calendarEvent);
            int changes = await _calendarEventRepository.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            _logger.LogInformation(
                $"Deleting calendar event with ID: {id} via Calendar Repository."
            );
            var calendarEvent = await _calendarEventRepository.GetEventByIdAsync(id);
            if (calendarEvent == null)
            {
                return false;
            }

            _calendarEventRepository.DeleteEvent(calendarEvent);
            int changes = await _calendarEventRepository.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<(bool IsInterested, int InterestedCount)?> ToggleInterestAsync(
            int eventId,
            string userId
        )
        {
            _logger.LogInformation(
                "ToggleInterestAsync kaldt med eventId: {EventId}, userId: {UserId}",
                eventId,
                userId
            );
            if (!int.TryParse(userId, out int parsedUserIdAsInt) || parsedUserIdAsInt == 0)
            {
                _logger.LogWarning(
                    "ToggleInterestAsync: Ugyldigt userIdString format eller værdi: {UserIdString}",
                    userId
                );
                return null;
            }
            var user = await _userManager.FindByIdAsync(userId);
            var calendarEvent = await _calendarEventRepository.GetEventByIdAsync(eventId);
            if (user == null || calendarEvent == null)
            {
                _logger.LogWarning(
                    "Event with ID {EventId} or User with ID {UserId} not found.",
                    eventId,
                    userId
                );
                return null;
            }

            var alreadyInterested = await _calendarEventRepository.RetrieveInterestPairsAsync(
                eventId,
                user.Id
            );
            bool isNowInterested;

            if (alreadyInterested != null)
            {
                await _calendarEventRepository.RemoveEventInterest(alreadyInterested);
                isNowInterested = false;
            }
            else
            {
                var userInterest = new EventInterest
                {
                    CalendarEventId = eventId,
                    UserId = user.Id,
                };

                await _calendarEventRepository.AddEventInterest(userInterest);
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
