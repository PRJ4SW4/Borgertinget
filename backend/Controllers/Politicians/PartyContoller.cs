using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.FT;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartyController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<PartyController> _logger;

    public PartyController(DataContext context, ILogger<PartyController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("Parties")]
    public async Task<ActionResult<IEnumerable<Party>>> getParties()
    {
        var parties = await _context.Party.OrderBy(p => p.partyName).ToListAsync();
        return Ok(parties);
    }

    [HttpGet("Party/{partyName}")]
    public async Task<ActionResult<Party>> GetPartyByName(string partyName) // Renamed parameter & method for clarity
    {
        if (string.IsNullOrWhiteSpace(partyName))
        {
            return BadRequest("Party name cannot be empty.");
        }

        try
        {
            // --- CORRECTED CODE ---
            // Use FirstOrDefaultAsync with a Where clause to filter by name
            var party = await _context.Party.FirstOrDefaultAsync(p =>
                p.partyName != null && p.partyName.ToLower() == partyName.ToLower()
            );
            // Added null check and ToLower() for case-insensitive matching, adjust if needed

            if (party == null)
            {
                // Use the actual name searched for
                return NotFound($"No party found with name '{partyName}'.");
            }
            return Ok(party);
        }
        catch (Exception ex)
        {
            // Use logger if available, otherwise Console.WriteLine for debugging
            Console.WriteLine($"Error fetching party with name {partyName}: {ex.Message}");
            // _logger.LogError(ex, "Error fetching party with name '{PartyName}'.", partyName); // If logger is injected
            return StatusCode(500, "An error occurred while fetching the party.");
        }
    }

    [HttpPut("Party/{partyId:int}")] // Using partyId in the route to identify the resource
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Party>> UpdatePartyDetails(
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
            var existingParty = await _context.Party.FindAsync(partyId);

            if (existingParty == null)
            {
                _logger.LogWarning("Party with ID {PartyId} not found for update.", partyId);
                return NotFound($"Party with ID {partyId} not found.");
            }

            // --- Apply Partial Updates ---
            bool changesMade = false;

            // Only update if the DTO property is not null
            if (updateDto.partyProgram != null)
            {
                // You might want to add length validation or other checks here
                if (existingParty.partyProgram != updateDto.partyProgram)
                {
                    existingParty.partyProgram = updateDto.partyProgram;
                    changesMade = true;
                    _logger.LogInformation(
                        "Updating PartyProgram for Party ID {PartyId}.",
                        partyId
                    );
                }
            }

            if (updateDto.history != null)
            {
                if (existingParty.history != updateDto.history)
                {
                    existingParty.history = updateDto.history;
                    changesMade = true;
                    _logger.LogInformation("Updating History for Party ID {PartyId}.", partyId);
                }
            }

            // Note: Property name matches the typo in the Party model.
            if (updateDto.politics != null)
            {
                if (existingParty.politics != updateDto.politics)
                {
                    existingParty.politics = updateDto.politics;
                    changesMade = true;
                    _logger.LogInformation("Updating Poilitics for Party ID {PartyId}.", partyId);
                }
            }

            // --- Save Changes (only if necessary) ---
            if (changesMade)
            {
                // EF Core automatically tracks changes to the loaded entity
                // _context.Party.Update(existingParty); // Usually not needed if entity is tracked
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Successfully updated details for Party ID {PartyId}.",
                    partyId
                );
            }
            else
            {
                _logger.LogInformation("No changes detected for Party ID {PartyId}.", partyId);
                // Return Ok with the unchanged entity if you prefer, or NoContent if no changes occurred.
                // Returning the entity is often useful.
            }

            // --- Return Success Response ---
            // Return 200 OK with the (potentially) updated party object
            return Ok(existingParty);
        }
        catch (DbUpdateConcurrencyException dbEx) // Handle potential concurrency issues
        {
            _logger.LogError(
                dbEx,
                "Concurrency error occurred while updating party ID {PartyId}.",
                partyId
            );
            return StatusCode(500, "A concurrency error occurred while updating the party.");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(
                dbEx,
                "Database error occurred while updating party ID {PartyId}.",
                partyId
            );
            return StatusCode(500, "A database error occurred while updating the party.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred while updating party ID {PartyId}.",
                partyId
            );
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
