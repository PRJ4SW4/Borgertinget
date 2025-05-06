// /backend/DTO/Flashcards/FlashcardCollectionDetailDTO.cs
namespace backend.DTO.Flashcards;

public class FlashcardCollectionDetailDTO
{
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Holds the list of individual flashcards for this collection
    public List<FlashcardDTO> Flashcards { get; set; } = new List<FlashcardDTO>();
}
