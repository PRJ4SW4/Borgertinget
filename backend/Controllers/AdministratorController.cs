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
}
