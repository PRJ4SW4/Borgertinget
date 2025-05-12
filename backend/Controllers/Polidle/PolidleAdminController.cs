using backend.Services;
using Microsoft.AspNetCore.Authorization; // <<< TILFØJET for Authorize
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization; // For CultureInfo
using System.Threading.Tasks;

namespace backend.Controllers
{
    #region Set-up
    /// Controller til administrative handlinger relateret til Polidle-spillet.
    /// Kræver Admin-rolle for adgang.
    [Route("api/polidle/admin")] // Base route for admin funktioner
    [ApiController]
    [Authorize(Roles = "Admin")] // <<< SIKRING: Kræver "Admin"-rolle for *alle* actions her
    public class PolidleAdminController : ControllerBase
    {
        private readonly IDailySelectionService _selectionService;
        private readonly ILogger<PolidleAdminController> _logger; // Logger for denne specifikke controller

        public PolidleAdminController(IDailySelectionService selectionService, ILogger<PolidleAdminController> logger)
        {
            _selectionService = selectionService;
            _logger = logger;
        }
    #endregion
    #region Todays selection
        /// Manuelt trigger generering og lagring af dagens Polidle-valg (alle gamemodes).
        /// <returns>Statuskode 200 OK ved succes, ellers 500 Internal Server Error.</returns>
        [HttpPost("generate-today")] // Route: POST api/polidle/admin/generate-today
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // Tilføjet typeof(string)
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GenerateDailySelectionForToday()
        {
            _logger.LogInformation("[Admin] Manual trigger received for generating today's selections.");
            try
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
                await _selectionService.SelectAndSaveDailyPoliticiansAsync(today);
                _logger.LogInformation("[Admin] Manual daily selection generation completed successfully for {Date}.", today);
                return Ok($"Daily selections generated successfully for {today:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Admin] Error occurred during manual daily selection generation for today.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred while generating daily selections."); // Mere generisk fejlbesked
            }
        }
    #endregion
    #region Specific date generate
        /// Manuelt trigger generering og lagring af Polidle-valg for en specifik dato.
        /// <param name="date">Den specifikke dato i formatet yyyy-MM-dd. Hvis udeladt eller ugyldig, bruges dags dato (UTC).</param>
        /// <returns>Statuskode 200 OK ved succes, ellers 500 Internal Server Error.</returns>
        [HttpPost("generate-specific-date")] // Route: POST api/polidle/admin/generate-specific-date
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // Tilføjet typeof(string)
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GenerateDailySelectionForDate([FromQuery] string? date = null)
        {
            DateOnly targetDate;
            if (!string.IsNullOrEmpty(date) && DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                targetDate = parsedDate;
                _logger.LogInformation("[Admin] Manual trigger received for generating selections for specific date: {Date}", targetDate);
            }
            else
            {
                targetDate = DateOnly.FromDateTime(DateTime.UtcNow);
                if (!string.IsNullOrEmpty(date)) // Log kun advarsel hvis der blev *forsøgt* at angive en dato
                {
                     _logger.LogWarning("[Admin] Invalid date format provided ('{ProvidedDate}'). Expected yyyy-MM-dd. Falling back to today: {FallbackDate}", date, targetDate);
                }
                else
                {
                    _logger.LogInformation("[Admin] Manual trigger received for generating today's selections (date not specified). Using {FallbackDate}", targetDate);
                }
            }

            try
            {
                await _selectionService.SelectAndSaveDailyPoliticiansAsync(targetDate);
                _logger.LogInformation("[Admin] Manual daily selection generation completed successfully for {Date}.", targetDate);
                return Ok($"Daily selections generated successfully for {targetDate:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Admin] Error during manual daily selection generation for {Date}.", targetDate);
                // Mere generisk fejlbesked
                return StatusCode(StatusCodes.Status500InternalServerError, $"An internal server error occurred while generating daily selections for {targetDate:yyyy-MM-dd}.");
            }
        }
    }
    #endregion
}