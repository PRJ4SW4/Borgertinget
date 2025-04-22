using backend.Models;
using backend.Services;
using backend.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic; // For List og KeyNotFoundException
using System; // For Convert for Base64

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

#region  politician-list
        // --- NYT: Endpoint til at hente alle politikere til gætte-input ---
            /// <summary>
            /// Controller for handling endpoints related to the Polidle game.
            /// Provides functionality to retrieve politicians, daily quotes, daily photos, 
            /// and process guesses made by users.
            /// </summary>
        [HttpGet("politicians")]
        [ProducesResponseType(typeof(List<PoliticianSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PoliticianSummaryDto>>> GetAllPoliticians()
        {
            _logger.LogInformation("Request received for all politician summaries.");
            var politicians = await _selectionService.GetAllPoliticiansForGuessingAsync();
            return Ok(politicians);
        }
#endregion

#region generate today

        // === START: NYT MANUELT TRIGGER ENDPOINT ===
        // POST /api/polidle/admin/generate-today
        [HttpPost("admin/generate-today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // Valgfrit: Skjul endpoint fra offentlig Swagger dokumentation
        // [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GenerateDailySelectionForToday()
        {
            _logger.LogInformation("Manual trigger received for generating today's selections.");
            try
            {
                // Hent dags dato (UTC for konsistens)
                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Kald service-metoden der vælger og gemmer
                await _selectionService.SelectAndSaveDailyPoliticiansAsync(today);

                _logger.LogInformation("Manual daily selection generation completed successfully for {Date}.", today);
                // Send et simpelt OK tilbage
                return Ok($"Daily selections generated successfully for {today:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during manual daily selection generation for today.");
                // Returner en serverfejl hvis noget går galt
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating daily selections.");
            }
        }
        // === SLUT: NYT MANUELT TRIGGER ENDPOINT ===

#endregion

#region Show todays quote

        // --- NYT: Endpoint til at hente dagens Citat ---
            /// <summary>
            /// Controller for handling endpoints related to the Polidle game.
            /// Provides functionality to retrieve politicians, daily quotes, daily photos, 
            /// and process guesses made by users.
            /// </summary>
        [HttpGet("quote/today")]
        [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuoteDto>> GetQuote()
        {
            _logger.LogInformation("Request received for today's quote.");
            try
            {
                var quoteDto = await _selectionService.GetQuoteOfTheDayAsync();
                return Ok(quoteDto);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning("Could not find today's quote selection: {Message}", knfex.Message);
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error getting today's quote.");
                 return StatusCode(StatusCodes.Status500InternalServerError, "Fejl ved hentning af dagens citat.");
            }
        }
#endregion

#region show todays photo
         // --- NYT: Endpoint til at hente dagens Foto ---
            /// <summary>
            /// Controller for handling endpoints related to the Polidle game.
            /// Provides functionality to retrieve politicians, daily quotes, daily photos, 
            /// and process guesses made by users.
            /// </summary>
        [HttpGet("photo/today")]
        [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PhotoDto>> GetPhoto()
        {
             _logger.LogInformation("Request received for today's photo.");
            try
            {
                var photoDto = await _selectionService.GetPhotoOfTheDayAsync();
                return Ok(photoDto);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning("Could not find today's photo selection: {Message}", knfex.Message);
                return NotFound(knfex.Message);
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Error getting today's photo.");
                 return StatusCode(StatusCodes.Status500InternalServerError, "Fejl ved hentning af dagens foto.");
            }
        }
#endregion

        // --- BEHOLDT: Endpoint til at håndtere Gæt ---
            /// <summary>
            /// Controller for handling endpoints related to the Polidle game.
            /// Provides functionality to retrieve politicians, daily quotes, daily photos, 
            /// and process guesses made by users.
            /// </summary>
        [HttpPost("guess")]
        [ProducesResponseType(typeof(GuessResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GuessResultDto>> PostGuess([FromBody] GuessRequestDto guessDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid guess request received: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Guess received for GameMode {GameMode} with GuessedPoliticianId {GuessedId}",
                guessDto.GameMode, guessDto.GuessedPoliticianId);

            try
            {
                var result = await _selectionService.ProcessGuessAsync(guessDto);
                return Ok(result);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning(knfex, "Could not process guess due to missing entity.");
                return NotFound(knfex.Message); // Send service-lagets fejlbesked
            }
            catch (InvalidOperationException ioex) // F.eks. hvis citat/foto mangler på politiker
            {
                _logger.LogWarning(ioex, "Could not process guess due to invalid state.");
                return BadRequest(ioex.Message); // Send service-lagets fejlbesked
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the guess for GameMode {GameMode}, GuessedId {GuessedId}",
                    guessDto.GameMode, guessDto.GuessedPoliticianId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Der opstod en fejl under behandling af dit gæt.");
            }
        }
    }
}