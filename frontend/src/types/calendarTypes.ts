// src/types/calendarTypes.ts

// Matches the backend CalendarEventDto
export interface CalendarEventDto {
    id: number;
    title: string;
    startDateTimeUtc: string; // Received as ISO 8601 string
    location?: string | null;
    sourceUrl?: string | null;
}

// Type expected by react-big-calendar components
export interface RbcEvent {
    id: number;
    title: string;
    start: Date; // JS Date object
    end: Date;   // JS Date object
    allDay?: boolean;
    resource?: any; // Can hold original event data or other info
}