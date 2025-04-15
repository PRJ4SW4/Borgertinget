public class FlashcardCollectionSummaryDto
{
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } // Included for sorting on frontend
}
