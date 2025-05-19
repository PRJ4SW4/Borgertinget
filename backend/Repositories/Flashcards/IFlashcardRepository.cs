namespace backend.Repositories.Flashcards;

using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Flashcards;

public interface IFlashcardRepository
{
    Task<IEnumerable<FlashcardCollection>> GetCollectionsAsync();
    Task<FlashcardCollection?> GetCollectionDetailsAsync(int collectionId);
}
