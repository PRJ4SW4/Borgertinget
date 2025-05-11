namespace backend.Services.AutomationServices.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data; // For DataContext
using backend.Models.Calendar; // For CalendarEvent
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Implements repository operations for CalendarEvent entities using Entity Framework Core.
public class CalendarEventRepository : ICalendarEventRepository
{
    private readonly DataContext _context; // Database context for data operations.
    private readonly ILogger<CalendarEventRepository> _logger; // Logger for recording repository activity.

    // Constructor: Takes injected DataContext and logger.
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
            .ToDictionaryAsync(e => e.SourceUrl, e => e); // Assuming SourceUrl is unique for future events.
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
            _logger.LogInformation(
                "Marked {Count} past calendar events (before {ThresholdUtc:O}) for deletion.",
                oldEventsToDelete.Count,
                utcThreshold
            );
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
        _logger.LogDebug(
            "Marked new calendar event for addition: Title='{EventTitle}', SourceUrl='{SourceUrl}'",
            newEvent.Title,
            newEvent.SourceUrl
        );
    }

    // Updates an existing CalendarEvent.
    public void UpdateEvent(CalendarEvent existingEvent)
    {
        _context.CalendarEvents.Update(existingEvent); // Mark for update.
        _logger.LogDebug(
            "Marked existing calendar event for update: ID={EventId}, Title='{EventTitle}'",
            existingEvent.Id,
            existingEvent.Title
        );
    }

    // Marks a collection of CalendarEvents for deletion.
    public void MarkEventsForDeletion(IEnumerable<CalendarEvent> eventsToDelete)
    {
        _context.CalendarEvents.RemoveRange(eventsToDelete); // Mark for deletion.
        _logger.LogDebug("Marked {Count} calendar events for deletion.", eventsToDelete.Count());
    }

    // Persists all pending changes.
    public async Task<int> SaveChangesAsync()
    {
        int changes = await _context.SaveChangesAsync(); // Save changes to the database.
        _logger.LogDebug("Persisted {DbChanges} changes to the database.", changes);
        return changes;
    }
}
