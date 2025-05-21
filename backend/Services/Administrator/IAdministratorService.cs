using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    public interface IAdministratorService
    {
        // Flashcard Collection
        Task<int> CreateCollectionAsync(FlashcardCollectionDetailDTO dto);
        Task UpdateCollectionInfoAsync(int collectionId, FlashcardCollectionDetailDTO dto);
        Task<List<string>> GetAllFlashcardCollectionTitlesAsync();
        Task<FlashcardCollectionDetailDTO> GetFlashCardCollectionByTitle(string title);
        Task DeleteFlashcardCollectionAsync(int collectionId);

        // Username
        Task<UserIdDTO> GetUserIdByUsernameAsync(string username);
        Task UpdateUserNameAsync(int userId, UpdateUserNameDto dto);

        // Politician Quotes
        Task<List<EditQuoteDTO>> GetAllQuotesAsync();
        Task<EditQuoteDTO> GetQuoteByIdAsync(int id);
        Task EditQuoteAsync(int quoteId, string quoteText);

        // Politician
        Task<int?> GetAktorIdByTwitterIdAsync(int twitterId);
    }
}
