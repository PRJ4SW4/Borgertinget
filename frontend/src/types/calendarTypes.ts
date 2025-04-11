// src/types/calendarTypes.ts

// Matches the backend CalendarEventDto
export interface CalendarEventDto {
    id: number;
    title: string;
    startDateTimeUtc: string; // Received as ISO 8601 string
    location?: string | null;
    sourceUrl?: string | null;
}

// --- Define a specific type for the 'resource' property ---
export interface RbcResource {
    location?: string | null;
    sourceUrl?: string | null;
    // Add any other custom data you might attach to an event here
}
// ---

// Type expected by react-big-calendar components
export interface RbcEvent {
    id: number;
    title: string;
    start: Date; // JS Date object
    end: Date;   // JS Date object
    allDay?: boolean;
    // --- Use the specific type instead of 'any' ---
    resource?: RbcResource;
    // ---
}
