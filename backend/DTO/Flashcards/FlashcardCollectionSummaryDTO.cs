// /backend/DTO/Flashcards/FlashcardCollectionSummaryDTO.cs
namespace backend.DTO.Flashcards;

public class FlashcardCollectionSummaryDTO
{
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } // Included for sorting of ordering on frontend
}
