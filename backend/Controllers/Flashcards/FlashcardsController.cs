namespace backend.Controllers;

using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO.Flashcards;
using backend.Services.Flashcards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class FlashcardsController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;

    public FlashcardsController(IFlashcardService flashcardService)
    {
        _flashcardService = flashcardService;
    }

    // Defines an HTTP GET endpoint to retrieve a summary list of flashcard collections for the sidebar.
    [HttpGet("collections")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<FlashcardCollectionSummaryDTO>>> GetCollections()
    {
        // Delegates the call to the service layer to fetch flashcard collection summaries.
        var collections = await _flashcardService.GetCollectionsAsync();
        // Returns an HTTP 200 OK response containing the list of summaries.
        return Ok(collections);
    }

    // Defines an HTTP GET endpoint to retrieve details of a specific flashcard collection, including its flashcards.
    // The 'id' parameter in the route corresponds to the collectionId.
    [HttpGet("collections/{id}")]
    [Authorize]
    public async Task<ActionResult<FlashcardCollectionDetailDTO>> GetCollectionDetails(int id)
    {
        // Delegates the call to the service layer to fetch details for the specified collection ID.
        var collectionDetails = await _flashcardService.GetCollectionDetailsAsync(id);

        // Checks if the service returned null, indicating the collection was not found.
        if (collectionDetails == null)
        {
            // Returns an HTTP 404 Not Found response if the collection does not exist.
            return NotFound($"Flashcard collection with ID {id} not found.");
        }
        // Returns an HTTP 200 OK response containing the detailed collection DTO.
        return Ok(collectionDetails);
    }
}
