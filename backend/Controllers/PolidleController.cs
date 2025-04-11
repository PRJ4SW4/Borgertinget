using backend.Models;
using backend.Services; // Service namespace
using backend.DTO;   // DTO namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolidleController : ControllerBase
    {
        private readonly IDailySelectionService _selectionService;
        private readonly ILogger<PolidleController> _logger;

        public PolidleController(IDailySelectionService selectionService, ILogger<PolidleController> logger)
        {
            _selectionService = selectionService;
            _logger = logger;
        }

        // GET: api/polidle/daily/Klassisk
        // GET: api/polidle/daily/1 (hvis du binder til int)
        [HttpGet("daily/{gameMode}")]
        [ProducesResponseType(typeof(DailyPoliticianDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DailyPoliticianDto>> GetDailyPolitician(GamemodeTypes gameMode)
        {
            // Enum binding håndterer typisk både streng og int
             _logger.LogInformation("Request received for daily politician for {GameMode}", gameMode);

            try
            {
                var politician = await _selectionService.GetOrSelectDailyPoliticianAsync(gameMode);

                if (politician == null)
                {
                    _logger.LogWarning("No daily politician could be determined for {GameMode}", gameMode);
                    return NotFound($"Kunne ikke finde eller vælge en dagens politiker for spiltype: {gameMode}.");
                }

                // Map til DTO - TILPAS DENNE MAPPING!
                var dto = new DailyPoliticianDto
                {
                    Id = politician.Id,
                    PolitikerNavn = politician.PolitikerNavn,
                    // Party = politician.Party, // Tilføj Party property til FakePolitiker
                    Region = politician.Region,
                    Køn = politician.Køn,
                    Alder = politician.Alder // Hvis Alder er gemt direkte, ellers beregn
                     // Map flere felter...
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting daily politician for {GameMode}", gameMode);
                // Returner en generisk fejl til klienten
                return StatusCode(StatusCodes.Status500InternalServerError, "Der opstod en intern serverfejl.");
            }
        }

        // --- Andre endpoints (f.eks. til at gætte) kommer her senere ---
        // [HttpPost("guess")]
        // public ActionResult<GuessResponseDto> PostGuess(...) { ... }

    }
}