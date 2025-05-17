namespace backend.Services.Flashcards;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.Flashcards;
using backend.Repositories.Flashcards;
using Microsoft.Extensions.Logging;

// Service responsible for handling business logic related to flashcards and flashcard collections.
public class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ILogger<FlashcardService> _logger;

    public FlashcardService(
        IFlashcardRepository flashcardRepository,
        ILogger<FlashcardService> logger
    )
    {
        _flashcardRepository = flashcardRepository;
        _logger = logger;
    }

    // Asynchronously retrieves a summary list of all flashcard collections.
    public async Task<IEnumerable<FlashcardCollectionSummaryDTO>> GetCollectionsAsync()
    {
        _logger.LogInformation("Fetching summary list of flashcard collections via repository.");
        var collections = await _flashcardRepository.GetCollectionsAsync();
        return collections.Select(c => new FlashcardCollectionSummaryDTO
        {
            CollectionId = c.CollectionId,
            Title = c.Title,
            DisplayOrder = c.DisplayOrder,
        });
    }

    // Asynchronously retrieves the detailed information for a specific flashcard collection.
    public async Task<FlashcardCollectionDetailDTO?> GetCollectionDetailsAsync(int collectionId)
    {
        _logger.LogInformation(
            "Fetching details for flashcard collection ID {CollectionId} via repository.",
            collectionId
        );
        var collection = await _flashcardRepository.GetCollectionDetailsAsync(collectionId);

        if (collection == null)
        {
            _logger.LogWarning(
                "Flashcard collection with ID {CollectionId} not found via repository.",
                collectionId
            );
            return null;
        }

        return new FlashcardCollectionDetailDTO
        {
            CollectionId = collection.CollectionId,
            Title = collection.Title,
            Description = collection.Description,
            Flashcards = collection
                .Flashcards.OrderBy(f => f.DisplayOrder)
                .Select(f => new FlashcardDTO
                {
                    FlashcardId = f.FlashcardId,
                    FrontContentType = f.FrontContentType.ToString(),
                    FrontText = f.FrontText,
                    FrontImagePath = f.FrontImagePath,
                    BackContentType = f.BackContentType.ToString(),
                    BackText = f.BackText,
                    BackImagePath = f.BackImagePath,
                })
                .ToList(),
        };
    }
}
