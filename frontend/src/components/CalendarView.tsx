// src/components/CalendarView.tsx
import { useState, useEffect, useMemo } from 'react';
// Removed react-big-calendar imports

// --- date-fns Imports ---
import { format } from 'date-fns';
import { da } from 'date-fns/locale';
// ---

// --- date-fns-tz Import ---
import { toZonedTime } from 'date-fns-tz';
// ---

import { fetchCalendarEvents } from '../services/ApiService'; // Adjust path if needed
import type { CalendarEventDto } from '../types/calendarTypes'; // Adjust path if needed

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

  // Fetch all events on initial mount
  useEffect(() => {
    setIsLoading(true);
    setError(null);

    // Fetch events - remove date range for now, fetch all or implement pagination later
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
            // Convert to Copenhagen time *before* formatting the date key
            const zonedDate = toZonedTime(utcDate, displayTimeZone);
            // Format date using Danish locale for the key (e.g., "11. april 2025")
            const dateKey = format(zonedDate, 'd. MMMM yyyy', { locale: da });

            if (!groups[dateKey]) {
                groups[dateKey] = [];
            }
            groups[dateKey].push(event);
        } catch (e) {
            console.error("Error processing date for event:", event, e);
            // Optionally handle events with invalid dates
        }
    });
    return groups;
  }, [events]); // Re-group only when the events array changes


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
      <h1>Kalender</h1>
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

              return (
                // List item for each event
                <li key={event.id} className="event-item">
                  {/* Link wrapping the event details */}
                  <a
                    href={eventUrl}
                    target="_blank" // Open in new tab
                    rel="noopener noreferrer" // Security best practice
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
                                <span className="location-icon" aria-hidden="true">📍</span> {/* Simple icon */}
                                {event.location}
                            </div>
                        )}
                    </div>
                  </a>
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