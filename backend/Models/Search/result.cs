using System.Collections.Generic;
using OpenSearch.Client;

namespace backend.Models
{
    public class SearchDocument
    {
        // Common fields
        public string Id { get; set; } = null!;
        public string DataType { get; set; } = null!;
        public string? Title { get; set; } 
        public string? Content { get; set; }
        public DateTime LastUpdated { get; set; }

        // --- Aktor specific fields ---
        public string? AktorName { get; set; }
        public string? Party { get; set; }
        public string? PartyShortname { get; set; }
        public string? PictureUrl { get; set; }
        public string? MinisterTitle { get; set; }
        public List<string>? Constituencies { get; set; }

        // --- Flashcard specific fields ---
        public int? FlashcardId { get; set; }
        public int? CollectionId { get; set; }
        public string? CollectionTitle { get; set; }
        public string? FrontText { get; set; } 
        public string? BackText { get; set; } 
        public string? FrontImagePath { get; set; }
        public string? BackImagePath { get; set; }

        // Suggestion field for autocomplete (optional but recommended)
        public CompletionField? Suggest { get; set; }

        public string? partyName {get; set;} = string.Empty;
        
        public string? partyShortNameFromParty {get; set;} = string.Empty;

        public string? partyProgram {get; set;} = string.Empty;

        public string? politics {get; set;} = string.Empty ;

        public string? history {get; set;} = string.Empty;

        //Page specific

        public string? pageTitle {get; set;} = string.Empty;
        public string? pageContent {get; set;} = string.Empty;
    }
}