// /backend/DTO/LearningEnvironment/FlashcardCollectionSummaryDTO.cs
namespace backend.DTO.LearningEnvironment;

public class FlashcardCollectionSummaryDTO
{
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } // Included for sorting of ordering on frontend
}
