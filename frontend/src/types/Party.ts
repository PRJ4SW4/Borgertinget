export interface IParty {
    partyId: number;
    partyName: string | null;
    partyShortName: string | null;
    partyProgram: string | null;
    politics: string | null; // Matches corrected backend model property
    history: string | null;
    stats: string[] | null; // Assuming stats is List<string>?
    chairmanId: number | null;
    viceChairmanId: number | null;
    secretaryId: number | null;
    spokesmanId: number | null;
    memberIds: number[] | null; // List of politician IDs
    // Add chairman, viceChairman etc. if you include nested objects in the backend response
  }