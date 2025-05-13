namespace backend.Services.Flashcards;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.Flashcards;
using backend.Models.Flashcards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Service responsible for handling business logic related to flashcards and flashcard collections.
public class FlashcardService : IFlashcardService
{
    // A private readonly field to hold the DataContext instance, enabling database interactions.
    private readonly DataContext _context;

    // A private readonly field for logging.
    private readonly ILogger<FlashcardService> _logger;

    // Constructor for the FlashcardService, injecting the DataContext and ILogger.
    public FlashcardService(DataContext context, ILogger<FlashcardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Asynchronously retrieves a summary list of all flashcard collections.
    public async Task<IEnumerable<FlashcardCollectionSummaryDTO>> GetCollectionsAsync()
    {
        _logger.LogInformation("Fetching summary list of flashcard collections.");
        // Asynchronously retrieves flashcard collections from the database.
        var collections = await _context
            .FlashcardCollections
            // Orders the collections primarily by their DisplayOrder for manual arrangement.
            .OrderBy(c => c.DisplayOrder)
            // Orders collections secondarily by their Title for alphabetical order within display groups as a fallback.
            .ThenBy(c => c.Title)
            // Projects the FlashcardCollection entities into FlashcardCollectionSummaryDTO objects.
            .Select(c => new FlashcardCollectionSummaryDTO
            {
                // Maps properties from the entity to the DTO.
                CollectionId = c.CollectionId,
                Title = c.Title,
                DisplayOrder = c.DisplayOrder,
            })
            // Executes the query and returns the results as a List.
            .ToListAsync();
        return collections;
    }

    // Asynchronously retrieves the detailed information for a specific flashcard collection.
    public async Task<FlashcardCollectionDetailDTO?> GetCollectionDetailsAsync(int collectionId)
    {
        _logger.LogInformation(
            "Fetching details for flashcard collection ID {CollectionId}.",
            collectionId
        );
        // Asynchronously retrieves a specific flashcard collection by its ID.
        // Includes its associated flashcards, ordered by their DisplayOrder.
        var collection = await _context
            .FlashcardCollections.Include(c => c.Flashcards) // Eager load flashcards
            .Where(c => c.CollectionId == collectionId)
            // Projects the FlashcardCollection entity into a FlashcardCollectionDetailDTO.
            .Select(c => new FlashcardCollectionDetailDTO
            {
                // Maps properties from the entity to the DTO.
                CollectionId = c.CollectionId,
                Title = c.Title,
                Description = c.Description,
                // Maps the collection of Flashcard entities to a list of FlashcardDTOs.
                // Order the flashcards here before projecting to DTOs.
                Flashcards = c
                    .Flashcards.OrderBy(f => f.DisplayOrder)
                    .Select(f => new FlashcardDTO
                    {
                        FlashcardId = f.FlashcardId,
                        FrontContentType = f.FrontContentType.ToString(), // Convert enum to string
                        FrontText = f.FrontText,
                        FrontImagePath = f.FrontImagePath,
                        BackContentType = f.BackContentType.ToString(), // Convert enum to string
                        BackText = f.BackText,
                        BackImagePath = f.BackImagePath,
                    })
                    .ToList(),
            })
            // Retrieves the first matching collection, or null if not found.
            .FirstOrDefaultAsync();

        if (collection == null)
        {
            _logger.LogWarning(
                "Flashcard collection with ID {CollectionId} not found.",
                collectionId
            );
        }
        // Returns the detailed DTO, or null if the collection was not found.
        return collection;
    }
}
