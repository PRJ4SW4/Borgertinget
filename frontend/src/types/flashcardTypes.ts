// Represents the summary data for listing collections in the sidebar
export interface FlashcardCollectionSummaryDto {
    collectionId: number;
    title: string;
    displayOrder: number;
}

// Represents the content type for a flashcard side
export type FlashcardContentType = "Text" | "Image";

// Represents a single flashcard's data as sent from the backend
export interface FlashcardDto {
    flashcardId: number;
    frontContentType: FlashcardContentType;
    frontText: string | null;
    frontImagePath: string | null; // Relative path like /uploads/flashcards/image.png
    backContentType: FlashcardContentType;
    backText: string | null;
    backImagePath: string | null; // Relative path
}

// Represents the detailed data for a single collection, including all its flashcards
export interface FlashcardCollectionDetailDto {
    collectionId: number;
    title: string;
    description: string | null;
    flashcards: FlashcardDto[]; // An array of the flashcards in this collection
}