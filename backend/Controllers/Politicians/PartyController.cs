using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using backend.Services;
using backend.Models; 
using backend.Data;
using backend.DTO.FT;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartyController : ControllerBase{
    private readonly DataContext _context;
    private readonly ILogger<PartyController> _logger;

    public PartyController(DataContext context, ILogger<PartyController> logger){
        _context = context;
        _logger = logger;
    }



    [HttpGet("Parties")]
    public async Task<ActionResult<IEnumerable<Party>>> getParties(){
        var parties = await _context.Party.
                                    OrderBy(p => p.partyName).
                                    ToListAsync();
        return Ok(parties);
    }
    
    [HttpGet("Party/{partyName}")]
    public async Task<ActionResult<Party>> GetPartyByName(string partyName)
    {
        if (string.IsNullOrWhiteSpace(partyName))
        {
            return BadRequest("Party name cannot be empty.");
        }

        try
        {
            var party = await _context.Party
                                    .FirstOrDefaultAsync(p => p.partyName != null && p.partyName.ToLower() == partyName.ToLower());

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
    public async Task<ActionResult<Party>> UpdatePartyDetails(int partyId, [FromBody] PartyDto updateDto)
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
            var existingParty = await _context.Party.FindAsync(partyId);

            if (existingParty == null)
            {
                _logger.LogWarning("Party with ID not found for update.");
                return NotFound($"Party with ID {partyId} not found.");
            }

            // --- Apply Partial Updates ---
            bool changesMade = false;

            // Only update if the DTO property is not null
            if (updateDto.partyProgram != null)
            {
                if (existingParty.partyProgram != updateDto.partyProgram)
                {
                    existingParty.partyProgram = updateDto.partyProgram;
                    changesMade = true;
                        _logger.LogInformation("Updating PartyProgram for Party ID.");
                }
            }

            if (updateDto.history != null)
            {
                    if (existingParty.history != updateDto.history)
                {
                    existingParty.history = updateDto.history;
                    changesMade = true;
                    _logger.LogInformation("Updating History for Party ID.");
                }
            }

            if (updateDto.politics != null)
            {
                    if (existingParty.politics != updateDto.politics)
                {
                    existingParty.politics = updateDto.politics;
                    changesMade = true;
                        _logger.LogInformation("Updating Poilitics for Party ID.");
                }
            }

            // --- Save Changes ---
            if (changesMade)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated details for Party ID.");
            }
            else
            {
                _logger.LogInformation("No changes detected for Party ID.");
            }

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