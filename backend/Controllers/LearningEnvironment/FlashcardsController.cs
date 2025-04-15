// /backend/Controllers/LearningEnvironment/FlashcardsController.cs
namespace backend.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Specifies the route for this controller, defining the base URL segment for its endpoints.
[Route("api/[controller]")]
// Indicates that this is an API controller, providing features like automatic HTTP 400 responses for invalid models.
[ApiController]
// [Authorize] // Add later if needed
public class FlashcardsController : ControllerBase
{
    // A private readonly field to hold the DataContext instance, enabling database interactions.
    private readonly DataContext _context; // Use your actual DbContext class name

    // Constructor for the FlashcardsController, injecting the DataContext via dependency injection.
    public FlashcardsController(DataContext context)
    {
        // Assigns the injected DataContext instance to the private field for use within the controller.
        _context = context;
    }

    // Defines an HTTP GET endpoint to retrieve a summary list of flashcard collections, typically for a sidebar or navigation menu.
    [HttpGet("collections")]
    public async Task<ActionResult<IEnumerable<FlashcardCollectionSummaryDTO>>> GetCollections()
    {
        // Asynchronously retrieves flashcard collections from the database, applying ordering for consistent presentation.
        var collections = await _context
            .FlashcardCollections
            // Orders the collections primarily by their DisplayOrder, allowing manual arrangement of collections.
            .OrderBy(c => c.DisplayOrder)
            // Orders collections secondarily by their Title, ensuring alphabetical order within each display order group.
            .ThenBy(c => c.Title)
            // Projects the retrieved FlashcardCollection entities into FlashcardCollectionSummaryDTO objects, shaping the data for the response.
            .Select(c => new FlashcardCollectionSummaryDTO
            {
                // Maps the CollectionId property from the FlashcardCollection entity to the DTO.
                CollectionId = c.CollectionId,
                // Maps the Title property from the FlashcardCollection entity to the DTO.
                Title = c.Title,
                // Maps the DisplayOrder property from the FlashcardCollection entity to the DTO.
                DisplayOrder = c.DisplayOrder,
            })
            // Executes the query and returns the results as a List.
            .ToListAsync();
        // Returns an HTTP 200 OK response containing the list of flashcard collection summaries.
        return Ok(collections);
    }

    // Defines an HTTP GET endpoint to retrieve detailed information for a specific flashcard collection, identified by its ID.
    [HttpGet("collections/{collectionId}")]
    public async Task<ActionResult<FlashcardCollectionDetailDTO>> GetCollectionDetails(
        int collectionId
    )
    {
        // Asynchronously retrieves a specific flashcard collection from the database, including its related flashcards.
        var collection = await _context
            .FlashcardCollections
            // Eagerly loads the Flashcards navigation property, ensuring related flashcards are retrieved in the same query.
            .Include(c => c.Flashcards.OrderBy(f => f.DisplayOrder))
            // Attempts to find a FlashcardCollection entity with the specified CollectionId.
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId);

        // Handles the case where the specified collection ID does not exist in the database.
        if (collection == null)
        {
            // Returns an HTTP 404 Not Found response with a descriptive message.
            return NotFound($"Collection with ID {collectionId} not found.");
        }

        // Maps the retrieved FlashcardCollection entity and its associated Flashcard entities to a FlashcardCollectionDetailDTO object.
        var collectionDetail = new FlashcardCollectionDetailDTO
        {
            // Maps the CollectionId property from the FlashcardCollection entity to the DTO.
            CollectionId = collection.CollectionId,
            // Maps the Title property from the FlashcardCollection entity to the DTO.
            Title = collection.Title,
            // Maps the Description property from the FlashcardCollection entity to the DTO.
            Description = collection.Description,
            // Maps each Flashcard entity to a FlashcardDTO, projecting the data into the desired format.
            Flashcards = collection
                .Flashcards.Select(f => new FlashcardDTO
                {
                    // Maps the FlashcardId property from the Flashcard entity to the DTO.
                    FlashcardId = f.FlashcardId,
                    // Converts the FrontContentType enum value to its string representation ("Text" or "Image").
                    FrontContentType = f.FrontContentType.ToString(),
                    // Maps the FrontText property from the Flashcard entity to the DTO.
                    FrontText = f.FrontText,
                    // Maps the FrontImagePath property from the Flashcard entity to the DTO.
                    FrontImagePath = f.FrontImagePath,
                    // Converts the BackContentType enum value to its string representation.
                    BackContentType = f.BackContentType.ToString(),
                    // Maps the BackText property from the Flashcard entity to the DTO.
                    BackText = f.BackText,
                    // Maps the BackImagePath property from the Flashcard entity to the DTO.
                    BackImagePath = f.BackImagePath,
                })
                // Converts the resulting IEnumerable<FlashcardDto> to a List.
                .ToList(),
        };

        // Returns an HTTP 200 OK response containing the detailed flashcard collection information.
        return Ok(collectionDetail);
    }

    // TODO: --- POST/PUT/DELETE methods for CRUD operations will be added later ---
}
