// Matches the backend CalendarEventDto
export interface CalendarEventDto {
    id: number;
    title: string;
    startDateTimeUtc: string; // Received as ISO 8601 string
    location?: string | null;
    sourceUrl?: string | null;
    interestedCount? : number;
    isCurrentUserInterested?: boolean;
}

// TODO: Delete as RBC didnt work
// --- A specific type for the 'resource' property ---
export interface RbcResource {
    location?: string | null;
    sourceUrl?: string | null;
}

// TODO: Delete as RBC didnt work 
// Type expected by react-big-calendar components
export interface RbcEvent {
    id: number;
    title: string;
    start: Date; // JS Date object
    end: Date;   // JS Date object
    allDay?: boolean;
    // Specific type, previously used any but typescript ofc does not allow that
    resource?: RbcResource;
}
