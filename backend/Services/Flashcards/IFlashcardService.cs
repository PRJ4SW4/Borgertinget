namespace backend.Services.Flashcards;

using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO.Flashcards;

// Defines a contract for services that handle flashcard-related operations.
public interface IFlashcardService
{
    // Asynchronously retrieves a summary list of all flashcard collections.
    // Suitable for displaying in a list or sidebar where full details are not immediately needed.
    Task<IEnumerable<FlashcardCollectionSummaryDTO>> GetCollectionsAsync();

    // Asynchronously retrieves the detailed information for a specific flashcard collection,
    // including all its flashcards.
    // Returns null if the collection with the specified ID is not found.
    Task<FlashcardCollectionDetailDTO?> GetCollectionDetailsAsync(int collectionId);
}
