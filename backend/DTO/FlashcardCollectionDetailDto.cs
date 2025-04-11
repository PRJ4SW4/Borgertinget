// Dtos/FlashcardCollectionDetailDto.cs

public class FlashcardCollectionDetailDto
{
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Holds the list of individual flashcards for this collection
    public List<FlashcardDto> Flashcards { get; set; } = new List<FlashcardDto>();
}
