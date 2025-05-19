using System; // For DateOnly
using System.Collections.Generic; // For List og KeyNotFoundException
using System.Threading.Tasks;
using backend.DTO;
using backend.Interfaces.Services;
using backend.Models; // For PoliticianSummaryDto (hvis den ligger her)
using backend.Services;
using backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    #region Set-up
    /// Controller til håndtering af endpoints for selve Polidle-spillet (klient-interaktion).
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PolidleController : ControllerBase
    {
        private readonly IDailySelectionService _selectionService;
        private readonly ILogger<PolidleController> _logger;

        public PolidleController(
            IDailySelectionService selectionService,
            ILogger<PolidleController> logger
        )
        {
            _selectionService = selectionService;
            _logger = logger;
        }
    #endregion
        #region List Poltician
        /// Henter en liste af politikere, som brugeren kan vælge imellem for at gætte.
        /// Kan filtreres med en søgestreng.
        /// <param name="search">Valgfri søgestreng til at filtrere politikernavne.</param>
        /// <returns>En liste af PoliticianSummaryDto objekter.</returns>
        [HttpGet("politicians")]
        [ProducesResponseType(typeof(List<SearchListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SearchListDto>>> GetAllPoliticians(
            [FromQuery] string? search = null
        )
        {
            // Rens 'search' før logning og før den sendes videre, HVIS den kun bruges til logning i service
            // Hvis service bruger den til DB query, skal den originale bruges dér, og servicen selv sanitize før logning.
            string sanitizedSearchForLog = LogSanitizer.Sanitize(search); // Rens kun til logning

            _logger.LogInformation(
                "Request received for politician summaries with search: '{SearchTerm}'.",
                sanitizedSearchForLog
            ); // <<< RETTET HER
            try
            {
                // Send den *originale* 'search' streng til servicen, da den skal bruges til at query databasen
                var politicians = await _selectionService.GetAllPoliticiansForGuessingAsync(search);
                return Ok(politicians);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching politicians with search term '{SearchTerm}'",
                    sanitizedSearchForLog
                ); // <<< RETTET HER
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred while fetching politicians."
                );
            }
        }
        #endregion
        #region Quote
        /// Henter dagens udvalgte citat til Citat-gamemode.
        /// <returns>Et QuoteDto objekt med dagens citat.</returns>
        [HttpGet("quote/today")]
        [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Hvis ingen selection findes for i dag
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Hvis data er inkonsistent (f.eks. manglende citat)
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                _logger.LogWarning(
                    "Could not find today's quote selection: {Message}",
                    knfex.Message
                );
                // Returner en mere generel besked evt.
                return NotFound("Today's quote selection could not be found.");
            }
            catch (InvalidOperationException ioex)
            {
                _logger.LogWarning(
                    "Could not retrieve quote due to invalid state: {Message}",
                    ioex.Message
                );
                // Returner en mere generel besked evt.
                return BadRequest("Could not retrieve today's quote due to inconsistent data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's quote.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred while getting today's quote."
                );
            }
        }
        #endregion
        #region Photo
        /// Henter URL'en til dagens udvalgte billede til Foto-gamemode.
        /// <returns>Et PhotoDto objekt med URL til dagens billede.</returns>
        [HttpGet("photo/today")]
        [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Hvis ingen selection findes for i dag
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Hvis data er inkonsistent (f.eks. manglende billede-URL)
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                _logger.LogWarning(
                    "Could not find today's photo selection: {Message}",
                    knfex.Message
                );
                return NotFound("Today's photo selection could not be found.");
            }
            catch (InvalidOperationException ioex)
            {
                _logger.LogWarning(
                    "Could not retrieve photo due to invalid state: {Message}",
                    ioex.Message
                );
                return BadRequest("Could not retrieve today's photo due to inconsistent data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's photo.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred while getting today's photo."
                );
            }
        }
        #endregion
        #region Classic
        /// Henter detaljerne for dagens udvalgte politiker til Classic-gamemode.
        /// Disse detaljer bruges som referencepunkt for brugerens gæt.
        /// <returns>Et PoliticianDetailsDto objekt med detaljer om dagens politiker.</returns>
        [HttpGet("classic/today")]
        [ProducesResponseType(typeof(DailyPoliticianDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Hvis ingen selection findes for i dag
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Hvis data er inkonsistent
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DailyPoliticianDto>> GetClassicDetails()
        {
            _logger.LogInformation("Request received for today's classic mode politician details.");
            try
            {
                // Antager at du tilføjer denne metode til IDailySelectionService
                var detailsDto = await _selectionService.GetClassicDetailsOfTheDayAsync();
                return Ok(detailsDto);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning(
                    "Could not find today's classic selection: {Message}",
                    knfex.Message
                );
                return NotFound("Today's classic politician selection could not be found.");
            }
            catch (InvalidOperationException ioex) // F.eks. hvis Aktor.Born ikke kunne parses til Alder
            {
                _logger.LogWarning(
                    "Could not retrieve classic details due to invalid state: {Message}",
                    ioex.Message
                );
                return BadRequest(
                    "Could not retrieve today's classic politician details due to inconsistent data."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's classic politician details.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred while getting today's classic politician details."
                );
            }
        }
        #endregion
        #region Guess
        /// Behandler et gæt fra en bruger for en specifik gamemode.
        /// <param name="guessDto">Data for gættet, inkl. gættet politiker ID og gamemode.</param>
        /// <returns>Et GuessResultDto objekt med feedback på gættet.</returns>
        [HttpPost("guess")]
        [ProducesResponseType(typeof(GuessResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Ved ugyldigt input eller inkonsistent data
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Hvis dagens valg eller gættet politiker ikke findes
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GuessResultDto>> PostGuess(
            [FromBody] GuessRequestDto guessDto
        )
        {
            // Input validering (fra DTO attributter)
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid guess request received: {ModelState}", ModelState);
                return BadRequest(ModelState); // Returnerer valideringsfejl
            }

            _logger.LogInformation(
                "Guess received for GameMode {GameMode} with GuessedPoliticianId {GuessedId}",
                guessDto.GameMode,
                guessDto.GuessedPoliticianId
            );

            try
            {
                var result = await _selectionService.ProcessGuessAsync(guessDto);
                return Ok(result);
            }
            catch (KeyNotFoundException knfex) // Kastet fra service hvis f.eks. GuessedPoliticianId eller DailySelection ikke findes
            {
                _logger.LogWarning(knfex, "Could not process guess due to missing entity.");
                return NotFound(knfex.Message); // Sender service-lagets besked (overvej om den er passende for klienten)
            }
            catch (InvalidOperationException ioex) // Kastet fra service hvis data er inkonsistent
            {
                _logger.LogWarning(ioex, "Could not process guess due to invalid state.");
                return BadRequest(ioex.Message); // Sender service-lagets besked (overvej om den er passende)
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while processing the guess for GameMode {GameMode}, GuessedId {GuessedId}",
                    guessDto.GameMode,
                    guessDto.GuessedPoliticianId
                );
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred while processing your guess."
                ); // Generisk besked
            }
        }
    }
        #endregion
}
