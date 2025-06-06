using backend.DTO.FT;
using backend.Services.Politicians;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

// TO DO: CHANGE CONSOLE.WriteLine -> use logger
[ApiController]
[Route("api/[controller]")]
public class AktorController : ControllerBase
{
    private readonly IFetchService _FetchService;
    private readonly IAktorService _aktorService;
    private readonly ILogger<AktorController> _logger; // Add Logger

    public AktorController(
        IFetchService fetcher,
        IAktorService aktorService,
        ILogger<AktorController> logger
    )
    {
        _FetchService = fetcher;
        _aktorService = aktorService;
        _logger = logger;
    }

    //Sender hele listen af politikere med bruger AktorDetailDto
    [HttpGet("all")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AktorDetailDto>>> GetAllAktors()
    {
        try
        {
            var aktors = await _aktorService.getAllAktors();

            return Ok(aktors);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occured while fetching aktors");
        }
    }

    //Sender en politiker, bruger AktorDetailDto
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<AktorDetailDto>> GetAktorById(int id)
    {
        try
        {
            var aktor = await _aktorService.getById(id);
            return Ok(aktor);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occured while processing your request");
        }
    }

    //sender politikere med samme partyName, bruger aktorDetailDto
    [HttpGet("GetParty/{partyName}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AktorDetailDto>>> GetParty(string partyName)
    {
        try
        {
            var aktors = await _aktorService.getByParty(partyName);
            return Ok(aktors);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("fetch")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAktorsFromExternal()
    {
        _logger.LogInformation(
            "[AktorController] Received request to update Aktors from external source."
        );
        try
        {
            var (added, updated, deleted) = await _FetchService.FetchAndUpdateAktorsAsync(); //kald service
            _logger.LogInformation(
                "[AktorController] Aktor update process completed. Added: {Added}, Updated: {Updated}, Deleted: {Deleted}",
                added,
                updated,
                deleted
            );
            return Ok(
                $"Successfully added {added}, updated {updated}, and deleted {deleted} aktors."
            );
        }
        catch (InvalidOperationException ioe)
        {
            _logger.LogError(ioe, "[AktorController] Configuration error during Aktor update.");
            return StatusCode(
                500,
                new
                {
                    message = "Server configuration error for Aktor update.",
                    error = ioe.Message,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[AktorController] An error occurred while updating Aktors from external source."
            );
            return StatusCode(
                500,
                new
                {
                    message = "An error occurred during the Aktor update process.",
                    error = ex.Message,
                }
            );
        }
    }
}
