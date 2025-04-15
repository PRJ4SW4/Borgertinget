// /backend/Controllers/LearningEnvironment/FlashcardsController.cs
namespace backend.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data; // Your DbContext namespace
using backend.DTO.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Import your DTOs and the FlashcardContentType enum if they are in different namespaces
// using backend.Dtos;
// using backend.Models;


[Route("api/[controller]")]
[ApiController]
// [Authorize] // Add later if needed
public class FlashcardsController : ControllerBase
{
    private readonly DataContext _context; // Use your actual DbContext class name

    public FlashcardsController(DataContext context)
    {
        _context = context;
    }

    // GET: api/flashcards/collections - Get list for sidebar
    [HttpGet("collections")]
    public async Task<ActionResult<IEnumerable<FlashcardCollectionSummaryDTO>>> GetCollections()
    {
        var collections = await _context
            .FlashcardCollections.OrderBy(c => c.DisplayOrder) // Order collections
            .ThenBy(c => c.Title) // Secondary sort by title
            .Select(c => new FlashcardCollectionSummaryDTO
            {
                // Map entity properties to DTO properties
                CollectionId = c.CollectionId,
                Title = c.Title,
                DisplayOrder = c.DisplayOrder,
            })
            .ToListAsync(); // Execute query
        return Ok(collections); // Return HTTP 200 OK with the list
    }

    // GET: api/flashcards/collections/{collectionId} - Get details for viewer
    [HttpGet("collections/{collectionId}")]
    public async Task<ActionResult<FlashcardCollectionDetailDTO>> GetCollectionDetails(
        int collectionId
    )
    {
        // Find the specific collection and include its related flashcards
        var collection = await _context
            .FlashcardCollections
            // Eager load the Flashcards navigation property
            // Also order the flashcards within the collection
            .Include(c => c.Flashcards.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId); // Find by ID

        // Handle case where the collection ID doesn't exist
        if (collection == null)
        {
            return NotFound($"Collection with ID {collectionId} not found."); // Return HTTP 404
        }

        // Map the found collection and its flashcards to the Detail DTO
        var collectionDetail = new FlashcardCollectionDetailDTO
        {
            CollectionId = collection.CollectionId,
            Title = collection.Title,
            Description = collection.Description,
            // Map each Flashcard entity to a FlashcardDto
            Flashcards = collection
                .Flashcards.Select(f => new FlashcardDTO
                {
                    FlashcardId = f.FlashcardId,
                    FrontContentType = f.FrontContentType.ToString(), // Convert Enum value to string ("Text" or "Image")
                    FrontText = f.FrontText,
                    FrontImagePath = f.FrontImagePath, // Assign the relative path directly
                    BackContentType = f.BackContentType.ToString(),
                    BackText = f.BackText,
                    BackImagePath = f.BackImagePath, // Assign the relative path directly
                })
                .ToList(), // Create the List<FlashcardDto>
        };

        return Ok(collectionDetail); // Return HTTP 200 OK with the details
    }

    // --- POST/PUT/DELETE methods for CRUD operations will be added later ---
}
