using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdministratorController : ControllerBase
{
    private readonly IAdministratorService _service;

    private readonly ILogger<AdministratorController> _logger;

    public AdministratorController(
        IAdministratorService service,
        ILogger<AdministratorController> logger
    )
    {
        _service = service;
        _logger = logger;
    }

    #region Flashcard Collection

    // POST Flashcard collection
    [HttpPost("PostFlashcardCollection")]
    public async Task<IActionResult> PostFlashCardCollection(FlashcardCollectionDetailDTO dto)
    {
        // If the incomming parameter is empty
        if (dto == null)
        {
            return BadRequest("No Collection to create from");
        }

        try
        {
            var collectionId = await _service.CreateCollectionAsync(dto);

            return Ok($"Flashcard Collection created with ID {collectionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating flashcard collection");
            return StatusCode(
                500,
                $"An error occurred while creating the collection: {ex.Message}"
            );
        }
    }

    // Upload image
    [HttpPost("UploadImage")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        // If the uploaded file is missing or empty
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        try
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "flashcards");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(file.FileName);

            // save full path wwwroot/uploads/flashcards/larsl.png
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path like: /uploads/flashcards/larsl.png
            var relativePath = $"/uploads/flashcards/{fileName}";
            return Ok(new { imagePath = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image");

            return StatusCode(500, $"An error occurred while uploading the file: {ex.Message}");
        }
    }

    // GET all flashcard collection titles
    [HttpGet("GetAllFlashcardCollectionTitles")]
    public async Task<IActionResult> GetFlashCardCollectionTitles()
    {
        try
        {
            // Fetches all Flashcard Collection titles
            var Titles = await _service.GetAllFlashcardCollectionTitlesAsync();

            return Ok(Titles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all Flashcard Collection titles");

            return StatusCode(500, $"Error Fetching Flashcard Collection titles: {ex.Message}");
        }
    }

    // GET flashcardCollection by title
    [HttpGet("GetFlashcardCollectionByTitle")]
    public async Task<IActionResult> GetFlashCardCollectionByTitle(string title)
    {
        try
        {
            var collection = await _service.GetFlashCardCollectionByTitle(title);

            return Ok(collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Flashcard Collection by title");

            return StatusCode(500, $"Error finding Flashcard Collection by title: {ex.Message}");
        }
    }

    // PUT Flashcard collection
    [HttpPut("UpdateFlashcardCollection/{collectionId}")]
    public async Task<IActionResult> UpdateFlashcardCollection(
        int collectionId,
        [FromBody] FlashcardCollectionDetailDTO dto
    )
    {
        if (dto == null)
        {
            return BadRequest("No collection data provided");
        }

        try
        {
            await _service.UpdateCollectionInfoAsync(collectionId, dto);
            return Ok("Flashcard collection updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Flashcard Collection with id");
            return StatusCode(
                500,
                $"An error occurred while updating the collection: {ex.Message}"
            );
        }
    }

    // DELETE FlashcardColletion
    [HttpDelete("DeleteFlashcardCollection")]
    public async Task<IActionResult> DeleteFlashcardCollection([FromQuery] int collectionId)
    {
        if (collectionId <= 0)
        {
            return BadRequest("Enter a valid ID");
        }

        try
        {
            await _service.DeleteFlashcardCollectionAsync(collectionId);

            return Ok($"Flashcard collection with ID {collectionId} deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Flashcard Collection with id");
            return StatusCode(
                500,
                $"An error occurred while deleting the Flashcard collection: {ex.Message}"
            );
        }
    }

    #endregion

    #region Brugernavn

    // GET Username ID
    [HttpGet("username")]
    public async Task<IActionResult> GetUsernameID(string username)
    {
        if (username == null)
        {
            return BadRequest("Enter valid username");
        }

        try
        {
            var userId = await _service.GetUserIdByUsernameAsync(username);

            return Ok(userId.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get username id with username");
            return StatusCode(500, $"An error occurred while getting the user: {ex.Message}");
        }
    }

    // PUT request for changing the username
    [HttpPut("{userId}")]
    public async Task<IActionResult> PutNewUserName(int userId, UpdateUserNameDto dto)
    {
        if (dto == null)
        {
            return BadRequest("No new username found");
        }

        try
        {
            await _service.UpdateUserNameAsync(userId, dto);

            return Ok("Username updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update username with id");
            return StatusCode(500, $"An error occurred while updating the username: {ex.Message}");
        }
    }

    #endregion

    #region Polidles Citat-mode

    // GET all politician quotes
    [HttpGet("GetAllQuotes")]
    public async Task<IActionResult> GetAllQuotes()
    {
        try
        {
            var quotes = await _service.GetAllQuotesAsync();

            return Ok(quotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all politician quotes");
            return StatusCode(
                500,
                $"An error occurred while getting all politician quotes: {ex.Message}"
            );
        }
    }

    // GET one quote instance by id
    [HttpGet("GetQuoteById")]
    public async Task<IActionResult> GetQuoteById(int quoteId)
    {
        try
        {
            var quote = await _service.GetQuoteByIdAsync(quoteId);

            return Ok(quote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get politician quote by id");
            return StatusCode(500, $"An error occurred while getting quote: {ex.Message}");
        }
    }

    // PUT a quoteText
    [HttpPut("EditQuote")]
    public async Task<IActionResult> EditQuote([FromBody] EditQuoteDTO dto)
    {
        try
        {
            await _service.EditQuoteAsync(dto.QuoteId, dto.QuoteText);

            return Ok("Quote edited");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to edit quote");
            return StatusCode(500, $"An error occurred while getting quote: {ex.Message}");
        }
    }

    #endregion

    #region Politician

    // GET Aktor id by Twitter id
    [HttpGet("lookup/aktorId")]
    public async Task<IActionResult> GetAktorIdByTwitterId([FromQuery] int twitterId)
    {
        if (twitterId <= 0)
            return BadRequest("Invalid Twitter ID.");

        try
        {
            int? aktorId = await _service.GetAktorIdByTwitterIdAsync(twitterId);

            if (aktorId == null)
                return NotFound($"No AktorId found for Twitter ID: {twitterId}");

            return Ok(new { aktorId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to look up AktorId for Twitter ID");
            return StatusCode(500, $"An error occurred while looking up AktorId: {ex.Message}");
        }
    }

    #endregion
}
