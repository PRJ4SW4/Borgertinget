using System;
using System.Collections.Generic;
using System.Linq;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AdministratorController : ControllerBase
{
    private readonly AdministratorService _service;

    public AdministratorController(AdministratorService service)
    {
        _service = service;
    }

    [HttpPost("PostFlashcardCollection")]
    public async Task<IActionResult> PostFlashCardCollection(FlashcardCollectionDetailDto dto)
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

    // Put request for changing the username
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

    [HttpDelete("{collectionId}")]
    public async Task<IActionResult> DeleteFlashcardCollection(int collectionId)
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
}
