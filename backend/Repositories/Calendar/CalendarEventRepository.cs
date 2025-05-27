namespace backend.Repositories.Calendar;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models.Calendar;
using backend.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Implements repository operations for CalendarEvent entities using Entity Framework Core.
public class CalendarEventRepository : ICalendarEventRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CalendarEventRepository> _logger;

    public CalendarEventRepository(DataContext context, ILogger<CalendarEventRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Retrieves future CalendarEvents.
    public async Task<Dictionary<string, CalendarEvent>> GetFutureEventsBySourceUrlAsync(
        DateTimeOffset utcThreshold
    )
    {
        _logger.LogDebug(
            "Fetching existing future calendar events from database (on or after {ThresholdUtc:O}).",
            utcThreshold
        );
        var events = await _context
            .CalendarEvents.Where(e => e.StartDateTimeUtc >= utcThreshold)
            .ToDictionaryAsync(e => e.SourceUrl, e => e);
        _logger.LogDebug("Found {Count} existing future calendar events.", events.Count);
        return events;
    }

    // Marks past CalendarEvents for deletion.
    public async Task<int> MarkPastEventsForDeletionAsync(DateTimeOffset utcThreshold)
    {
        var oldEventsToDelete = await _context
            .CalendarEvents.Where(e => e.StartDateTimeUtc < utcThreshold)
            .ToListAsync();

        if (oldEventsToDelete.Any())
        {
            _context.CalendarEvents.RemoveRange(oldEventsToDelete); // Mark for deletion.
            return oldEventsToDelete.Count;
        }
        _logger.LogInformation(
            "No past calendar events found for deletion (before {ThresholdUtc:O}).",
            utcThreshold
        );
        return 0;
    }

    // Adds a new CalendarEvent.
    public async Task AddEventAsync(CalendarEvent newEvent)
    {
        await _context.CalendarEvents.AddAsync(newEvent); // Mark for addition.
    }

    // Retrieves a specific CalendarEvent by its ID.
    public async Task<CalendarEvent?> GetEventByIdAsync(int id)
    {
        _logger.LogDebug(
            "Fetching calendar event by ID: {EventId}",
            LogSanitizer.Sanitize(id.ToString())
        );
        var calendarEvent = await _context.CalendarEvents.FindAsync(id);
        if (calendarEvent == null)
        {
            _logger.LogWarning(
                "Calendar event with ID: {EventId} not found.",
                LogSanitizer.Sanitize(id.ToString())
            );
        }
        return calendarEvent;
    }

    // Updates an existing CalendarEvent.
    public void UpdateEvent(CalendarEvent existingEvent)
    {
        _context.CalendarEvents.Update(existingEvent); // Mark for update.
    }

    // Marks a collection of CalendarEvents for deletion.
    public void MarkEventsForDeletion(IEnumerable<CalendarEvent> eventsToDelete)
    {
        _context.CalendarEvents.RemoveRange(eventsToDelete); // Mark for deletion.
        _logger.LogDebug("Marked {Count} calendar events for deletion.", eventsToDelete.Count());
    }

    // Marks a single CalendarEvent for deletion.
    public void DeleteEvent(CalendarEvent eventToDelete)
    {
        _context.CalendarEvents.Remove(eventToDelete);
        _logger.LogDebug(
            "Marked calendar event for deletion: ID={EventId}",
            LogSanitizer.Sanitize(eventToDelete.Id.ToString())
        );
    }

    // Persists all pending changes.
    public async Task<int> SaveChangesAsync()
    {
        int changes = await _context.SaveChangesAsync(); // Save changes to the database.
        _logger.LogDebug("Persisted {DbChanges} changes to the database.", changes);
        return changes;
    }

    // Retrieves all CalendarEvents.
    public async Task<IEnumerable<CalendarEvent>> GetAllEventsAsync()
    {
        _logger.LogInformation("Fetching all calendar events from database.");
        var events = await _context
            .CalendarEvents.Include(ce => ce.InterestedUsers)
            .OrderBy(e => e.StartDateTimeUtc)
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation("Found {EventCount} total events.", events.Count);
        return events;
    }

    // Retrieves EventInterests for a specific CalendarEvent and user.
    // Returns null if the event or user is not found.
    public async Task<EventInterest?> RetrieveInterestPairsAsync(int eventId, int userId)
    {
        var calendarEvent = await _context.CalendarEvents.FindAsync(eventId);
        if (calendarEvent == null)
        {
            _logger.LogWarning("Calendar event with ID {EventId} not found.", eventId);
            return null;
        }

        var alreadyInterested = await _context
            .EventInterests.Where(ei => ei.CalendarEventId == eventId && ei.UserId == userId)
            .FirstOrDefaultAsync();

        return alreadyInterested;
    }

    public async Task AddEventInterest(EventInterest eventInterest)
    {
        _context.EventInterests.Add(eventInterest);
        _logger.LogDebug(
            "Marked new event interest for addition: EventId={EventId}, UserId={UserId}",
            eventInterest.CalendarEventId,
            eventInterest.UserId
        );
        await Task.CompletedTask;
    }

    public async Task RemoveEventInterest(EventInterest eventInterest)
    {
        _context.EventInterests.Remove(eventInterest);
        _logger.LogDebug(
            "Marked event interest for deletion: EventId={EventId}, UserId={UserId}",
            eventInterest.CalendarEventId,
            eventInterest.UserId
        );
        await Task.CompletedTask;
    }

    public async Task<int> GetInterestedUsersAsync(int eventId)
    {
        var interestedUsers = await _context
            .EventInterests.Where(ei => ei.CalendarEventId == eventId)
            .ToListAsync();
        return interestedUsers.Count;
    }
}
