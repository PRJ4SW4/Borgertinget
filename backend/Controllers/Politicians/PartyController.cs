using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.FT;
using backend.Models;
using backend.Models.Politicians;
using backend.Services.Politicians;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartyController : ControllerBase
{
    private readonly IPartyService _service;
    private readonly ILogger<PartyController> _logger;

    public PartyController(IPartyService service, ILogger<PartyController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("Parties")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Party>>> getParties()
    {
        var parties = await _service.GetAll();

        if (parties == null)
        {
            return StatusCode(404, "No parties found");
        }
        return Ok(parties);
    }

    [HttpGet("Party/{partyName}")]
    [Authorize]
    public async Task<ActionResult<Party>> GetPartyByName(string partyName)
    {
        if (string.IsNullOrWhiteSpace(partyName))
        {
            return BadRequest("Party name cannot be empty.");
        }

        try
        {
            var party = await _service.GetByName(partyName);

            if (party == null)
            {
                return NotFound($"No party found with name '{partyName}'.");
            }
            return Ok(party);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching party'.");
            return StatusCode(500, "An error occurred while fetching the party.");
        }
    }

    [HttpPut("Party/{partyId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PartyDto>> UpdatePartyDetails(
        int partyId,
        [FromBody] PartyDto updateDto
    )
    {
        // --- Input Validation ---
        if (updateDto == null)
        {
            return BadRequest("Update data cannot be null.");
        }

        if (partyId <= 0)
        {
            return BadRequest("Invalid Party ID.");
        }

        // --- Fetch Existing Entity ---
        try
        {
            var existingParty = await _service.UpdateDetails(partyId, updateDto);

            // --- Return Success Response ---
            return Ok(existingParty);
        }
        catch (DbUpdateConcurrencyException dbEx) // Handle potential concurrency issues
        {
            _logger.LogError(dbEx, "Concurrency error occurred while updating party ID.");
            return StatusCode(500, "A concurrency error occurred while updating the party.");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while updating party ID.");
            return StatusCode(500, "A database error occurred while updating the party.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating party ID.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
