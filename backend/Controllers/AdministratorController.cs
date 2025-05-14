using System;
using System.Collections.Generic;
using System.Linq;
using backend.Data;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdministratorController : ControllerBase
{
    private readonly IAdministratorService _service;

    public AdministratorController(IAdministratorService service)
    {
        _service = service;
    }

    #region Flashcard Collection

    // POST Flashcard collection
    [HttpPost("PostFlashcardCollection")]
    public async Task<IActionResult> PostFlashCardCollection(FlashcardCollectionDetailDTO dto)
    {
        if (dto == null)
        {
            return BadRequest("No Collection to create from");
        }

        try
        {
            // Use service to create a Flashcard collection into the Db
            var collectionId = await _service.CreateCollectionAsync(dto);

            return Ok($"Flashcard Collection created with ID {collectionId}");
        }
        catch (Exception ex)
        {
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
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var uploadsFolder = Path.Combine("wwwroot", "uploads", "flashcards");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = Path.GetFileName(file.FileName);

        // save full path wwwroot/uploads/flashcards/larsl.png
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);

            // Return relative path like: /uploads/flashcards/larsl.png
            var relativePath = $"/uploads/flashcards/{fileName}";
            return Ok(new { imagePath = relativePath });
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
            return StatusCode(500, $"Error Fetching Flashcard Collection titles: {ex}");
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
            return StatusCode(500, $"Error finding Flashcard Collection by title: {ex}");
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
            return StatusCode(
                500,
                $"An error occured while deleting the Flashcard collection: {ex.Message}"
            );
        }
    }

    #endregion

    #region Brugernavn

    // GET all users
    [HttpGet("GetAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _service.GetAllUsersAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occured while getting users: {ex.Message}");
        }
    }

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
            var user = await _service.GetUserByUsernameAsync(username);

            return Ok(user.Id);
        }
        catch
        {
            return StatusCode(500, $"An error occured while getting the user: {username}");
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
        catch
        {
            return StatusCode(500, "An error occured while getting all politician quotes ");
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
        catch
        {
            return StatusCode(500, $"An error occured while getting quote with id: {quoteId} ");
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
        catch
        {
            return StatusCode(500, $"An error occured while editing quote with id: {dto.QuoteId}");
        }
    }

    #endregion

    #region Politician

    [HttpGet("lookup/aktorId")]
    public async Task<IActionResult> GetAktorIdByTwitterId([FromQuery] int twitterId)
    {
        if (twitterId <= 0)
            return BadRequest("Ugyldigt Twitter ID.");

        int? aktorId = await _service.GetAktorIdByTwitterIdAsync(twitterId);

        if (aktorId == null)
            return NotFound($"Ingen AktorId fundet for Twitter ID {twitterId}");

        return Ok(new { aktorId });
    }

    #endregion
}
