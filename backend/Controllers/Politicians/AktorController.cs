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
using backend.Services.Politician;
using Microsoft.Extensions.Configuration;
namespace backend.Controllers;

// TO DO: CHANGE CONSOLE.WriteLine -> use logger
[ApiController]
[Route("api/[controller]")]
public class AktorController : ControllerBase{
    private readonly DataContext _context;
    private readonly HttpService _httpService;
    private readonly IConfiguration _configuration;
    private readonly IFetchService _FetchService;
    private readonly ILogger<AktorController> _logger; // Add Logger

    public AktorController(DataContext context, HttpService httpService, IConfiguration conf, IFetchService fetcher ,ILogger<AktorController> logger){
        _context = context;
        _httpService = httpService;
        _configuration = conf;
        _FetchService = fetcher;
        _logger = logger;
    }
    //Sender hele listen af politikere med bruger AktorDetailDto
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Aktor>>> GetAllAktors(){
        var aktors = await _context.Aktor.
                                          Where(a => a.typeid ==5).
                                          OrderBy(a => a.navn).
                                          Select(a => AktorDetailDto.FromAktor(a)).
                                          ToListAsync();

        return Ok(aktors);
    }
    //Sender en politiker, bruger AktorDetailDto
    [HttpGet("{id}")]
    public async Task<ActionResult<Aktor>> GetAktorById(int id)
    {
        try
        {
            var aktor = await _context.Aktor.FindAsync(id);

            if (aktor == null)
            {
                // Return 404 Not Found if no match
                return NotFound($"No politician found with ID {id}.");
            }
            var aktorDto = AktorDetailDto.FromAktor(aktor);
            // Return the found politician with 200 OK status
            return Ok(aktorDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching politician with ID {id}: {ex.Message}");
            // Return a generic 500 Internal Server Error
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    //sender politikere med samme partyName, bruger aktorDetailDto
    [HttpGet("GetParty/{partyName}")]
    public async Task<ActionResult<IEnumerable<Aktor>>> GetParty(string partyName)
    {
        // Basic validation for the input party name
        if (string.IsNullOrWhiteSpace(partyName))
        {
            return BadRequest("Party name cannot be empty."); // Return 400 Bad Request if name is missing
        }

        // Normalize the input party name to lower case for case-insensitive comparison
        var lowerPartyName = partyName.ToLower();

        try
        {
            // Query the database for Aktors
            var filteredPoliticians = await _context.Aktor
                // Filter where either Party or PartyShortname matches (case-insensitive)
                .Where(a => (a.Party != null && a.Party.ToLower() == lowerPartyName) ||
                            (a.PartyShortname != null && a.PartyShortname.ToLower() == lowerPartyName))
                .OrderBy(a => a.navn)
                .Select(a => AktorDetailDto.FromAktor(a))
                .ToListAsync();

            return Ok(filteredPoliticians);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching politicians for party {partyName}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
    
    //https://oda.ft.dk/api/Akt%C3%B8r?$inlinecount=allpages endpoint der skal bruges i hvert fald
    //----------------------------------------//
    //                To DO:                  //
    //                                        //
    //                                        //
    //                                        //
    //----------------------------------------//

    [HttpGet("fetch")] // Changed to POST if it modifies data, GET if it only fetches and returns status
        public async Task<IActionResult> UpdateAktorsFromExternal()
        {
            _logger.LogInformation("[AktorController] Received request to update Aktors from external source.");
            try
            {
                var (added, updated, deleted) = await _FetchService.FetchAndUpdateAktorsAsync();
                _logger.LogInformation("[AktorController] Aktor update process completed. Added: {Added}, Updated: {Updated}, Deleted: {Deleted}", added, updated, deleted);
                return Ok($"Successfully added {added}, updated {updated}, and deleted {deleted} aktors.");
            }
            catch (InvalidOperationException ioe) // Catch specific configuration errors
            {
                _logger.LogError(ioe, "[AktorController] Configuration error during Aktor update.");
                return StatusCode(500, new { message = "Server configuration error for Aktor update.", error = ioe.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AktorController] An error occurred while updating Aktors from external source.");
                return StatusCode(500, new { message = "An error occurred during the Aktor update process.", error = ex.Message });
            }
        }
    
}



