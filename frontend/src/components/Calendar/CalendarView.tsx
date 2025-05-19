import { useState, useEffect, useMemo, useCallback } from 'react';
import { format } from 'date-fns';
import { da } from 'date-fns/locale';
import { toZonedTime } from 'date-fns-tz';
import { fetchCalendarEvents, toggleEventInterest } from '../../services/ApiService';
import type { CalendarEventDto } from '../../types/calendarTypes';

// Import CSS for this component
import './CalendarView.css';

// Timezone for Display
const displayTimeZone = 'Europe/Copenhagen';
const altingetBaseUrl = 'https://www.altinget.dk';

// --- Main Calendar Component ---
function CalendarView() {
  // Use CalendarEventDto directly from API
  const [events, setEvents] = useState<CalendarEventDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [togglingInterestFor, setTogglingInterestFor] = useState<number | null>(null);

  // Fetch all events on initial mount
  useEffect(() => {
    setIsLoading(true);
    setError(null);

    // Fetch events
    fetchCalendarEvents()
      .then(data => {
        // Sort fetched data by date before grouping
        const sortedData = data.sort((a, b) =>
            new Date(a.startDateTimeUtc).getTime() - new Date(b.startDateTimeUtc).getTime()
        );
        setEvents(sortedData);
      })
      .catch(err => {
        setError(err.message || "Failed to load events");
        console.error("Error fetching calendar events:", err);
      })
      .finally(() => {
        setIsLoading(false);
      });

  // Empty dependency array means fetch only once on mount
  }, []);


  // Group events by date using useMemo for efficiency
  const groupedEvents = useMemo(() => {
    const groups: Record<string, CalendarEventDto[]> = {};
    if (!events) return groups;

    events.forEach(event => {
        try {
            const utcDate = new Date(event.startDateTimeUtc);
            // Convert to Copenhagen time before formatting the date key
            const zonedDate = toZonedTime(utcDate, displayTimeZone);
            // Format date for the key (e.g., "11. april 2025")
            const dateKey = format(zonedDate, 'd. MMMM yyyy', { locale: da });

            if (!groups[dateKey]) {
                groups[dateKey] = [];
            }
            groups[dateKey].push(event);
        } catch (e) {
            console.error("Error processing date for event:", event, e);
        }
    });
    return groups;
  }, [events]); // Re-group only when the events array changes

  // --- Event Handler for Interest Toggle ---
  const handleToggleInterest = useCallback(async (eventId: number) => {
    try {
      const updatedInterestData = await toggleEventInterest(eventId);
      setEvents(prevEvents =>
        prevEvents.map(event =>
          event.id === eventId
            ? {
                ...event,
                isCurrentUserInterested: updatedInterestData.isInterested,
                interestedCount: updatedInterestData.interestedCount
              }
            : event
        )
      );
    } catch (error: unknown) {
      console.error("Fejl ved opdatering af interesse:", error);
      let errorMessage = "Kunne ikke opdatere din interesse. Prøv igen."; 

      if (typeof error === 'object' && error !== null) {
        const potentialApiError = error as { response?: { data?: { message?: string } }, message?: string };

        if (potentialApiError.response?.data?.message && typeof potentialApiError.response.data.message === 'string') {
          errorMessage = potentialApiError.response.data.message;
        } else if (potentialApiError.message && typeof potentialApiError.message === 'string') {
          errorMessage = potentialApiError.message;
        }
      }
      else if (typeof error === 'string') {
          errorMessage = error;
      }
  setError(errorMessage);
    } finally {
      setTogglingInterestFor(null);
    }
  }, [setEvents, setTogglingInterestFor, setError]);

  // --- Render Logic ---
  if (isLoading) {
    return <div className="calendar-list-status">Henter begivenheder...</div>;
  }

  if (error) {
    return <div className="calendar-list-status error">Fejl: {error}</div>;
  }

  const dateKeys = Object.keys(groupedEvents); // Get the dates for rendering headings

  if (dateKeys.length === 0) {
    return <div className="calendar-list-status">Ingen begivenheder fundet.</div>;
  }

  return (
    <div className="calendar-list-container">
      {/* Iterate through each date group */}
      {dateKeys.map((dateKey) => (
        <div key={dateKey} className="date-group">
          {/* Date Heading */}
          <h3 className="date-heading">{dateKey}</h3>
          {/* List of events for this date */}
          <ul className="event-list">
            {groupedEvents[dateKey].map(event => {
              // Format time in Copenhagen timezone
              const zonedStartTime = toZonedTime(new Date(event.startDateTimeUtc), displayTimeZone);
              const formattedTime = format(zonedStartTime, 'HH:mm');
              // Construct the full URL to Altinget
              const eventUrl = event.sourceUrl ? `${altingetBaseUrl}${event.sourceUrl}` : '#'; // Fallback href if URL missing
              const isLoadingInterest = togglingInterestFor === event.id;
              return (
                // List item for each event
                <li key={event.id} className="event-item">
                  {/* Link wrapping the event details */}
                  <a
                    href={eventUrl}
                    target="_blank" // Open in new tab
                    rel="noopener noreferrer"
                    className="event-link"
                    title={`Se på Altinget: ${event.title}`} // Tooltip
                  >
                    {/* Event Time */}
                    <div className="event-time">{formattedTime}</div>
                    {/* Event Content (Title & Location) */}
                    <div className="event-content">
                        <div className="event-title">{event.title}</div>
                        {/* Display location if available */}
                        {event.location && (
                            <div className="event-location">
                                <span className="location-icon" aria-hidden="true">📍</span> {/* Location icon */}
                                {event.location}
                            </div>
                        )}
                    </div>
                  </a>
                  <div className="event-interest">
                    <button
                      className={`interest-toggle-button ${event.isCurrentUserInterested ? 'interested' : 'not-interested'}`}
                      onClick={() => handleToggleInterest(event.id)}
                      aria-label={event.isCurrentUserInterested ? 'Deltag ikke' : 'Deltag'}
                      aria-pressed={event.isCurrentUserInterested}>
                        {isLoadingInterest ? 'Opdaterer...' : event.isCurrentUserInterested ? 'Deltager' : 'Deltag'}
                    </button>
                    <div className="event-interest-count">
                      {event.interestedCount} {event.interestedCount === 1 ? 'person' : 'personer'} deltager
                    </div>
                  </div>
                </li>
              );
            })}
          </ul>
        </div>
      ))}
    </div>
  );
}

export default CalendarView;