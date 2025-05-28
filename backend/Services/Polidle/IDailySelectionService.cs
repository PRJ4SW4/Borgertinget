using backend.DTO;

namespace backend.Interfaces.Services
{
    public interface IDailySelectionService
    {
        Task<List<SearchListDto>> GetAllPoliticiansForGuessingAsync(string? search = null);
        Task<QuoteDto?> GetQuoteOfTheDayAsync();
        Task<PhotoDto?> GetPhotoOfTheDayAsync();
        Task<DailyPoliticianDto?> GetClassicDetailsOfTheDayAsync();
        Task<GuessResultDto?> ProcessGuessAsync(GuessRequestDto guessDto);
        Task SelectAndSaveDailyPoliticiansAsync(DateOnly date, bool overwriteExisting = false);
        Task<string> SeedQuotesForAllAktorsAsync();

        //TODO: Create an endpoint for adding a Quote to a specific Aktor (future work)
    }
}
