using backend.Models;
using backend.Models.Flashcards;

public interface IAdministratorRepository
{
    // Flashcard collections
    Task AddFlashcardCollectionAsync(FlashcardCollection collection);
    Task UpdateFlashcardCollectionAsync(FlashcardCollection collection);
    Task DeleteFlashcardCollectionAsync(FlashcardCollection collection);
    Task<FlashcardCollection?> GetFlashcardCollectionByIdAsync(int id);
    Task<FlashcardCollection?> GetFlashcardCollectionByTitleAsync(string title);
    Task<List<string>> GetAllFlashcardCollectionTitlesAsync();

    // Users
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User[]> GetAllUsersAsync();
    Task UpdateUserAsync(User user);

    // Quotes
    Task<List<PoliticianQuote>> GetAllQuotesAsync();
    Task<PoliticianQuote?> GetQuoteByIdAsync(int id);
    Task UpdateQuoteAsync(PoliticianQuote quote);

    // Politician
    Task<int?> GetAktorIdByTwitterIdAsync(int twitterId);

}
