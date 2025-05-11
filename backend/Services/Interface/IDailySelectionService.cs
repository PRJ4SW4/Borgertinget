using System; // For DateOnly
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO;
using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Interface for services related to selecting daily Polidle items and processing guesses.
    /// </summary>
    public interface IDailySelectionService
    {
        /// <summary>
        /// Henter eller udvælger og gemmer dagens politiker for en given spiltype.
        /// Sørger for at relatere data (som parti) er inkluderet.
        /// </summary>
        /// <param name="gameMode">Spiltypen der hentes for.</param>
        /// <returns>Dagens FakePolitiker objekt med parti inkluderet, eller null hvis ingen kunne findes/vælges.</returns>
        Task<FakePolitiker?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode);

        // --- OPDATERET METODE SIGNATUR ---
        /// <summary>
        /// Henter en liste af politikere (summary) til brug for gætte-input (autocomplete).
        /// Listen kan filtreres baseret på en søgestreng.
        /// </summary>
        /// <param name="search">Valgfri søgestreng til at filtrere på politikernavn (case-insensitive 'contains').</param>
        /// <returns>En liste af matchende politikere (begrænset antal) med ID, navn og portræt.</returns>
        Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync(string? search = null);

        // ---------------------------------

        /// <summary>
        /// Henter dagens citat (placeholder - implementering mangler).
        /// </summary>
        Task<QuoteDto> GetQuoteOfTheDayAsync(); // Antager QuoteDto findes eller skal laves

        /// <summary>
        /// Henter dagens foto (placeholder - implementering mangler).
        /// </summary>
        Task<PhotoDto> GetPhotoOfTheDayAsync(); // Antager PhotoDto findes eller skal laves

        /// <summary>
        /// Behandler et gæt fra en bruger mod dagens politiker for den givne spiltype.
        /// </summary>
        /// <param name="guessDto">Data for gættet (gættet politiker ID, spiltype).</param>
        /// <returns>Et resultatobjekt med detaljeret feedback.</returns>
        /// <exception cref="KeyNotFoundException">Kastes hvis den gættede politiker eller dagens politiker ikke findes.</exception>
        Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto);

        /// <summary>
        /// Udvælger og gemmer dagens politikere for alle spiltyper for en given dato.
        /// Denne metode kaldes typisk af en baggrundsjob/scheduled task.
        /// </summary>
        /// <param name="date">Datoen der skal vælges for.</param>
        Task SelectAndSaveDailyPoliticiansAsync(DateOnly date);
    }
}
