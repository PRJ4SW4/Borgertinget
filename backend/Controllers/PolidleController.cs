using backend.Models;
using backend.Services; // Service namespace
using backend.DTO;    // DTO namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic; // Tilføjet for KeyNotFoundException

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

        // GET: api/polidle/daily/{gameMode}
        // (Din eksisterende GetDailyPolitician metode er her...)
        [HttpGet("daily/{gameMode}")]
        [ProducesResponseType(typeof(DailyPoliticianDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DailyPoliticianDto>> GetDailyPolitician(GamemodeTypes gameMode)
        {
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
                     // Sørg for at FakeParti er loaded! Ellers er politician.FakeParti null
                     // PartiNavn = politician.FakeParti?.PartiNavn ?? "Ukendt Parti", // Eksempel
                     Region = politician.Region,
                     Køn = politician.Køn,
                     Alder = politician.Alder
                 };

                return Ok(dto);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "An error occurred while getting daily politician for {GameMode}", gameMode);
                return StatusCode(StatusCodes.Status500InternalServerError, "Der opstod en intern serverfejl.");
            }
        }


        // --- NYT ENDPOINT TIL AT HÅNDTERE GÆT ---
        [HttpPost("guess")]
        [ProducesResponseType(typeof(GuessResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GuessResultDto>> PostGuess([FromBody] GuessRequestDto guessDto)
        {
            // Tjekker automatisk for [Required] attributter pga. [ApiController]
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid guess request received: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Guess received for GameMode {GameMode} with GuessedPoliticianId {GuessedId}",
                guessDto.GameMode, guessDto.GuessedPoliticianId);

            try
            {
                // Kald service-laget til at behandle gættet
                var result = await _selectionService.ProcessGuessAsync(guessDto);
                return Ok(result);
            }
            catch (KeyNotFoundException knfex) // Specifik fejl hvis politiker/daglig valg ikke findes
            {
                _logger.LogWarning(knfex, "Could not process guess due to missing entity.");
                // Returner 404 Not Found hvis enten dagens politiker eller den gættede ikke findes
                return NotFound(knfex.Message); // Send service-lagets fejlbesked til klienten
            }
            catch (Exception ex) // Generel fejlhåndtering
            {
                _logger.LogError(ex, "An error occurred while processing the guess for GameMode {GameMode}, GuessedId {GuessedId}",
                    guessDto.GameMode, guessDto.GuessedPoliticianId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Der opstod en fejl under behandling af dit gæt.");
            }
        }
        // -----------------------------------------
    }
}