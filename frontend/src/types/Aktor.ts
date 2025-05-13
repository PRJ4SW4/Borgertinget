// src/interfaces/Aktor.ts
export interface IAktor {
    // --- Direct fields from backend model ---
    id: number;
    navn: string | null; // Match nullability from backend model
    fornavn: string | null;
    efternavn: string | null;
    biografi: string | null; // The raw XML/tagged string (optional to display)
    opdateringsdato: string;
    ministertitel?: string | null;
  
    // --- Parsed fields from biography ---
    // (Assuming your backend endpoint adds these)
    party?: string | null;
    partyShortname?: string | null;
    sex?: string | null;
    born?: string | null;
    educationStatistic?: string | null;
    pictureMiRes?: string | null;
    email?: string | null;
    functionFormattedTitle?: string | null;
    functionStartDate?: string | null;
    currentConstituency?: string | null;
    parliamentaryPositionsOfTrust?: string[] | null;
    positionsOfTrust?: string[] | null;
    publicationsText?: string | null; // Full text if needed
  
    // --- List fields (originally stored as JSON strings in DB) ---
    // Assuming backend parses or EF Core handles conversion correctly
    // so the API returns actual arrays:
    constituencies?: string[] | null;
    nominations?: string[] | null;
    educations?: string[] | null;
    occupations?: string[] | null;
    publicationTitles?: string[] | null;
    spokesmen?: string[] | null;
    ministers?: string[] | null;
  
    // If your API returns JSON strings like "constituenciesJson",
    // you would define those here instead as:
    // constituenciesJson?: string | null;
    // nominationsJson?: string | null;
    // educations?: string | null;
    // occupations?: string | null;
    // publicationTitles?: string | null;
    // etc.
    // And then parse them in the component.

    
  }