using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;
using backend.Models.Flashcards;
using backend.Repositories;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    /// <summary>
    /// Business‑logic layer for administrator actions. All persistence is delegated to
    /// IAdministratorRepository This service handles validation and mapping
    /// between DTOs and EF Core entities.
    /// </summary>
    public class AdministratorService : IAdministratorService
    {
        private readonly IAdministratorRepository _repository;

        public AdministratorService(IAdministratorRepository repository)
        {
            _repository = repository;
        }

        #region Flashcard Collection

        // POST Flashcard collection
        public async Task<int> CreateCollectionAsync(FlashcardCollectionDetailDTO dto)
        {
            var collection = MapDtoToEntity(dto);
            await _repository.AddFlashcardCollectionAsync(collection);

            return collection.CollectionId;
        }

        // PUT Flashcard Collection
        public async Task UpdateCollectionInfoAsync(
            int collectionId,
            FlashcardCollectionDetailDTO dto
        )
        {
            var collection = await _repository.GetFlashcardCollectionByIdAsync(collectionId);
            if (collection == null)
            {
                throw new KeyNotFoundException(
                    $"Flashcard Collection with ID {collectionId} not found"
                );
            }

            // Update collection title and description
            collection.Title = dto.Title;
            collection.Description = dto.Description;

            // Clear and replace all flashcards
            collection.Flashcards.Clear();
            foreach (var fc in dto.Flashcards)
            {
                collection.Flashcards.Add(MapFlashcardDtoToEntity(fc));
            }

            await _repository.UpdateFlashcardCollectionAsync(collection);
        }

        // Get List of Flashcard Collection Titles
        public async Task<List<string>> GetAllFlashcardCollectionTitlesAsync()
        {
            var titles = await _repository.GetAllFlashcardCollectionTitlesAsync();

            if (titles.Count == 0)
            {
                throw new KeyNotFoundException("No Flashcard Collection titles found");
            }

            return titles;
        }

        // Get Flashcard collection by Title
        public async Task<FlashcardCollectionDetailDTO> GetFlashCardCollectionByTitle(string title)
        {
            var collection = await _repository.GetFlashcardCollectionByTitleAsync(title);

            if (collection == null)
            {
                throw new KeyNotFoundException(
                    $"Flashcard Collection with title: {title} not found"
                );
            }

            // Map entity to DTO
            return MapEntityToDto(collection);
        }

        // DELETE Flashcard collection
        public async Task DeleteFlashcardCollectionAsync(int collectionId)
        {
            var collection = await _repository.GetFlashcardCollectionByIdAsync(collectionId);

            if (collection == null)
            {
                throw new KeyNotFoundException(
                    $"Flashcard collection with ID {collectionId} not found"
                );
            }

            await _repository.DeleteFlashcardCollectionAsync(collection);
        }

        #endregion

        #region User

        // GET user by username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var user = await _repository.GetUserByUsernameAsync(username);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {username} not found");
            }

            return user;
        }

        // GET all users
        public async Task<User[]> GetAllUsersAsync()
        {
            var users = await _repository.GetAllUsersAsync();

            if (users.Length == 0)
            {
                throw new KeyNotFoundException("Error finding users");
            }

            return users;
        }

        // PUT changing a Users username
        public async Task UpdateUserNameAsync(int userId, UpdateUserNameDto dto)
        {
            var user = await _repository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} is not found");
            }

            // Update username
            user.UserName = dto.UserName;
            user.NormalizedUserName = dto.UserName.ToUpper();

            await _repository.UpdateUserAsync(user);
        }

        #endregion

        #region Citat mode

        // GET all quotes
        public async Task<List<EditQuoteDTO>> GetAllQuotesAsync()
        {
            var quotes = await _repository.GetAllQuotesAsync();

            if (quotes.Count == 0)
            {
                throw new KeyNotFoundException("Error finding Politician Quotes");
            }

            // Initialize DTO list
            return quotes
                .Select(q => new EditQuoteDTO { QuoteId = q.QuoteId, QuoteText = q.QuoteText })
                .ToList();
        }

        // GET one quote instance by id
        public async Task<EditQuoteDTO> GetQuoteByIdAsync(int id)
        {
            var quote = await _repository.GetQuoteByIdAsync(id);

            if (quote == null)
            {
                throw new KeyNotFoundException("Error quote not found");
            }

            return new EditQuoteDTO { QuoteId = quote.QuoteId, QuoteText = quote.QuoteText };
        }

        // PUT a quoteText
        public async Task EditQuoteAsync(int quoteId, string quoteText)
        {
            var quote = await _repository.GetQuoteByIdAsync(quoteId);

            if (quote == null)
            {
                throw new KeyNotFoundException($"Error quote with quoteID: {quoteId} not found");
            }

            quote.QuoteText = quoteText;
            await _repository.UpdateQuoteAsync(quote);
        }

        #endregion

        #region Helper mapping

        private static FlashcardCollection MapDtoToEntity(FlashcardCollectionDetailDTO dto)
        {
            var collection = new FlashcardCollection
            {
                Title = dto.Title,
                Description = dto.Description,
                DisplayOrder = 0,
                Flashcards = dto.Flashcards.Select(MapFlashcardDtoToEntity).ToList(),
            };
            return collection;
        }

        private static Flashcard MapFlashcardDtoToEntity(FlashcardDTO fc)
        {
            Enum.TryParse(fc.FrontContentType, out FlashcardContentType frontType);
            Enum.TryParse(fc.BackContentType, out FlashcardContentType backType);

            return new Flashcard
            {
                FrontContentType = frontType,
                FrontText = fc.FrontText,
                FrontImagePath = fc.FrontImagePath,
                BackContentType = backType,
                BackText = fc.BackText,
                BackImagePath = fc.BackImagePath,
            };
        }

        private static FlashcardCollectionDetailDTO MapEntityToDto(FlashcardCollection entity)
        {
            return new FlashcardCollectionDetailDTO
            {
                CollectionId = entity.CollectionId,
                Title = entity.Title,
                Description = entity.Description,
                Flashcards = entity
                    .Flashcards.Select(fc => new FlashcardDTO
                    {
                        FlashcardId = fc.FlashcardId,
                        FrontContentType = fc.FrontContentType.ToString(),
                        FrontText = fc.FrontText,
                        FrontImagePath = fc.FrontImagePath,
                        BackContentType = fc.BackContentType.ToString(),
                        BackText = fc.BackText,
                        BackImagePath = fc.BackImagePath,
                    })
                    .ToList(),
            };
        }

        #endregion

        #region Politician

        // Region: Politician Twitter lookup
        public async Task<int?> GetAktorIdByTwitterIdAsync(int twitterId)
        {
            if (twitterId <= 0)
                throw new ArgumentOutOfRangeException(nameof(twitterId), "Ugyldigt Twitter ID.");

            int? aktorId = await _repository.GetAktorIdByTwitterIdAsync(twitterId);

            return aktorId;
        }

        #endregion
    }
}
