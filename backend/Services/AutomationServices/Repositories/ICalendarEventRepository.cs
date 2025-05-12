namespace backend.Services.AutomationServices.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Calendar; // For CalendarEvent

// Defines a contract for repository operations related to CalendarEvent entities.
public interface ICalendarEventRepository
{
    // Retrieves a dictionary of existing future CalendarEvents, keyed by their SourceUrl.
    // Events are considered "future" if their StartDateTimeUtc is on or after the provided utcThreshold.
    Task<Dictionary<string, CalendarEvent>> GetFutureEventsBySourceUrlAsync(
        DateTimeOffset utcThreshold
    );

    // Marks past CalendarEvents for deletion.
    // Events are considered "past" if their StartDateTimeUtc is before the provided utcThreshold.
    // Returns the count of events marked for deletion.
    Task<int> MarkPastEventsForDeletionAsync(DateTimeOffset utcThreshold);

    // Adds a new CalendarEvent to the data store.
    Task AddEventAsync(CalendarEvent newEvent);

    // Updates an existing CalendarEvent in the data store.
    void UpdateEvent(CalendarEvent existingEvent); // EF Core tracks changes, so this might not need to be async if just modifying properties.

    // Marks a collection of CalendarEvents for deletion.
    void MarkEventsForDeletion(IEnumerable<CalendarEvent> eventsToDelete);

    // Persists all pending changes to the data store.
    // Returns the number of state entries written to the database.
    Task<int> SaveChangesAsync();
}
