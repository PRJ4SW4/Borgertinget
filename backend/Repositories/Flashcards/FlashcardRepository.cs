namespace backend.Repositories.Flashcards;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models.Flashcards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class FlashcardRepository : IFlashcardRepository
{
    private readonly DataContext _context;
    private readonly ILogger<FlashcardRepository> _logger;

    public FlashcardRepository(DataContext context, ILogger<FlashcardRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FlashcardCollection>> GetCollectionsAsync()
    {
        _logger.LogInformation("Fetching summary list of flashcard collections from repository.");
        return await _context
            .FlashcardCollections.OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Title)
            .ToListAsync();
    }

    public async Task<FlashcardCollection?> GetCollectionDetailsAsync(int collectionId)
    {
        _logger.LogInformation(
            "Fetching details for flashcard collection ID {CollectionId} from repository.",
            collectionId
        );

        var collectionDetails = await _context
            .FlashcardCollections.Include(c => c.Flashcards)
            .Where(c => c.CollectionId == collectionId)
            .FirstOrDefaultAsync();

        return collectionDetails;
    }
}
