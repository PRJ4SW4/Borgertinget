namespace backend.Repositories.Calendar;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Calendar;

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

    // Retrieves a specific CalendarEvent by its ID.
    Task<CalendarEvent?> GetEventByIdAsync(int id); // Added nullable return type

    // Updates an existing CalendarEvent in the data store.
    void UpdateEvent(CalendarEvent existingEvent); // EF Core tracks changes, so this might not need to be async if just modifying properties.

    // Marks a collection of CalendarEvents for deletion.
    void MarkEventsForDeletion(IEnumerable<CalendarEvent> eventsToDelete);

    // Marks a single CalendarEvent for deletion.
    void DeleteEvent(CalendarEvent eventToDelete);

    // Persists all pending changes to the data store.
    // Returns the number of state entries written to the database.
    Task<int> SaveChangesAsync();

    // Method to retrieve all CalendarEvents from the data store.
    // This method is asynchronous and returns a collection of CalendarEvent objects.
    Task<IEnumerable<CalendarEvent>> GetAllEventsAsync();

    Task<EventInterest?> RetrieveInterestPairsAsync(int eventId, int userId);

    Task AddEventInterest(EventInterest eventInterest);

    Task RemoveEventInterest(EventInterest eventInterest);

    Task<int> GetInterestedUsersAsync(int eventId);
}
