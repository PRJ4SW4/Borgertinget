using System.Collections.Generic;

namespace backend.Models
{
    public class SearchDocument
    {
        // Common fields
        public string Id { get; set; } = null!; // Unique ID (e.g., "aktor-123", "flashcard-456")
        public string DataType { get; set; } = null!; // "Aktor" or "Flashcard"
        public string? Title { get; set; } // A primary searchable title (e.g., Aktor name, Flashcard front text)
        public string? Content { get; set; } // Main searchable content (e.g., Bio, Party, Flashcard back text)
        public DateTime LastUpdated { get; set; } // Timestamp for when it was indexed

        // --- Aktor specific fields ---
        public string? AktorName { get; set; } // Keep original name field if needed for display
        public string? Party { get; set; }
        public string? PartyShortname { get; set; }
        public string? PictureUrl { get; set; }
        public string? MinisterTitle { get; set; }

        // Add other relevant Aktor fields you want to search or display directly...
        public List<string>? Constituencies { get; set; } // Example list field

        // --- Flashcard specific fields ---
        public int? FlashcardId { get; set; } // Original ID
        public int? CollectionId { get; set; }
        public string? CollectionTitle { get; set; }
        public string? FrontText { get; set; } // Keep original separate if needed
        public string? BackText { get; set; } // Keep original separate if needed
        public string? FrontImagePath { get; set; }
        public string? BackImagePath { get; set; }

        // Suggestion field for autocomplete (optional but recommended)
        // public CompletionField Suggest { get; set; }
    }
}
