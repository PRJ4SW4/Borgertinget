// Fil: IDailySelectionService.cs
using backend.DTO;    // Tilføj for DTOs
using backend.Models; // Tilføj for Models
using System.Threading.Tasks;

namespace backend.Services // Eller dit service-namespace
{
    public interface IDailySelectionService
    {
        /// <summary>
        /// Henter eller udvælger dagens politiker for en given spiltype.
        /// </summary>
        Task<FakePolitiker?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode);

        /// <summary>
        /// Behandler et gæt fra en bruger mod dagens politiker for den givne spiltype.
        /// </summary>
        Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto);
    }
}