export interface SearchDocument {
  id: string;
  dataType: string;
  title?: string | null;
  content?: string | null;
  lastUpdated: string;

  aktorName?: string | null;
  party?: string | null;
  partyShortname?: string | null;
  pictureUrl?: string | null;
  ministerTitle?: string | null;
  constituencies?: string[] | null;

  flashcardId?: number | null; // This might be null if ID is embedded in the main 'id'
  collectionId?: number | null;
  collectionTitle?: string | null;
  frontText?: string | null;
  backText?: string | null;
  frontImagePath?: string | null;
  backImagePath?: string | null;

  partyName?: string | null;
  partyShortNameFromParty: string | null;
  partyProgram: string | null;
  politics: string | null;
  history: string | null;


}