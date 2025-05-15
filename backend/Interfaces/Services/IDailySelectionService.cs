using System; // For DateOnly
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO; // Namespace for DTOs
using backend.Models; // Namespace for GamemodeTypes (eller backend.Enums, hvis du flyttede den)

namespace backend.Interfaces.Services // Sørg for at namespacet matcher din projektstruktur
{
    /// <summary>
    /// Interface for services, der håndterer logik for Polidle-spillet,
    /// inklusiv daglige valg, hentning af spildata og behandling af gæt.
    /// </summary>
    public interface IDailySelectionService
    {
        /// <summary>
        /// Henter en liste af politikere (summaries) til brug i frontend gætte-input (f.eks. autocomplete).
        /// Kan filtreres via en søgestreng.
        /// </summary>
        /// <param name="search">Valgfri søgestreng til at filtrere på politikernavn.</param>
        /// <returns>En asynkron opgave, der resulterer i en liste af matchende <see cref="SearchListDto"/> (tidl. DailyPoliticianDto for summary).</returns>
        Task<List<SearchListDto>> GetAllPoliticiansForGuessingAsync(string? search = null);

        /// <summary>
        /// Henter dagens udvalgte citat til Citat-gamemode.
        /// </summary>
        /// <returns>En asynkron opgave, der resulterer i en <see cref="QuoteDto"/> for dagens citat.</returns>
        /// <exception cref="KeyNotFoundException">Kastes hvis ingen DailySelection findes for dagens dato og Citat-mode.</exception>
        /// <exception cref="InvalidOperationException">Kastes hvis den fundne DailySelection mangler citat-tekst.</exception>
        Task<QuoteDto> GetQuoteOfTheDayAsync();

        /// <summary>
        /// Henter dagens udvalgte foto (som URL) til Foto-gamemode.
        /// </summary>
        /// <returns>En asynkron opgave, der resulterer i en <see cref="PhotoDto"/> med URL til dagens billede.</returns>
        /// <exception cref="KeyNotFoundException">Kastes hvis ingen DailySelection eller tilhørende Aktor findes for dagens dato og Foto-mode.</exception>
        /// <exception cref="InvalidOperationException">Kastes hvis den fundne Aktor mangler en billede-URL.</exception>
        Task<PhotoDto> GetPhotoOfTheDayAsync();

        /// <summary>
        /// Henter detaljerne for dagens udvalgte politiker til Classic-gamemode.
        /// </summary>
        /// <returns>En asynkron opgave, der resulterer i en <see cref="DailyPoliticianDto"/> (detaljeret DTO, tidl. PoliticianDetailsDto) for dagens politiker.</returns>
        /// <exception cref="KeyNotFoundException">Kastes hvis ingen DailySelection eller tilhørende Aktor findes for dagens dato og Classic-mode.</exception>
        /// <exception cref="InvalidOperationException">Kastes hvis nødvendige data for DTO'en mangler (f.eks. fødselsdato til aldersberegning).</exception>
        Task<DailyPoliticianDto> GetClassicDetailsOfTheDayAsync();

        /// <summary>
        /// Behandler et gæt fra en bruger mod dagens udvalgte politiker for den givne spiltype.
        /// </summary>
        /// <param name="guessDto">DTO med data for gættet (gættet politiker ID, spiltype).</param>
        /// <returns>En asynkron opgave, der resulterer i en <see cref="GuessResultDto"/> med detaljeret feedback.</returns>
        /// <exception cref="KeyNotFoundException">Kastes hvis den gættede politiker eller dagens DailySelection ikke findes.</exception>
        /// <exception cref="InvalidOperationException">Kastes hvis nødvendige data for sammenligning mangler.</exception>
        Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto);

        /// <summary>
        /// Udfører logikken til at udvælge og gemme dagens politikere for *alle* spiltyper for en given dato.
        /// </summary>
        /// <param name="date">Datoen der skal genereres valg for.</param>
        /// <param name="overwriteExisting">Hvis true, slettes eksisterende valg for datoen før nye genereres.</param>
        /// <returns>En asynkron opgave, der fuldføres når processen er afsluttet.</returns>
        Task SelectAndSaveDailyPoliticiansAsync(DateOnly date, bool overwriteExisting = false); // <<< TILFØJET overwriteExisting

        //TODO: Create an endpoint for seeding Aktor with Quotes
        /* SOLUTION:
        * Add two(2) quotes to all Aktor-objects in the DB.
        * Only adds if it DOES NOT have two quotes
        * <returns>Statusmessage about operation</returns>
        */
        Task<string> SeedQuotesForAllAktorsAsync();

        //TODO: Create an endpoint for adding a Quote to a specific Aktor
    }
}
