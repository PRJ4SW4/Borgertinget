using backend.Models;
using backend.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // For DateOnly

namespace backend.Services
{
    public interface IDailySelectionService
    {
        Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync();
        Task<QuoteDto> GetQuoteOfTheDayAsync();
        Task<PhotoDto> GetPhotoOfTheDayAsync();
        Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto);

        // Denne metode kaldes typisk af en baggrundsjob/scheduled task
        Task SelectAndSaveDailyPoliticiansAsync(DateOnly date);
    }
}