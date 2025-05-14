using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Models.Flashcards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Repositories
{
    /// <summary>
    /// Repository that performs **pure data‑access** operations for administrator‑related
    /// entities.  All business/validation logic lives in the AdministratorService.
    /// </summary>
    public class AdministratorRepository : IAdministratorRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<AdministratorRepository> _logger;

        public AdministratorRepository(DataContext context, ILogger<AdministratorRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Flashcard collections

        public async Task AddFlashcardCollectionAsync(FlashcardCollection collection)
        {
            _context.FlashcardCollections.Add(collection);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFlashcardCollectionAsync(FlashcardCollection collection)
        {
            _context.Entry(collection).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFlashcardCollectionAsync(FlashcardCollection collection)
        {
            _context.FlashcardCollections.Remove(collection);
            await _context.SaveChangesAsync();
        }

        public async Task<FlashcardCollection?> GetFlashcardCollectionByIdAsync(int id)
        {
            return await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync(c => c.CollectionId == id);
        }

        public async Task<FlashcardCollection?> GetFlashcardCollectionByTitleAsync(string title)
        {
            return await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync(c => c.Title == title);
        }

        public async Task<List<string>> GetAllFlashcardCollectionTitlesAsync()
        {
            return await _context.FlashcardCollections.Select(c => c.Title).ToListAsync();
        }

        #endregion

        #region Users

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User[]> GetAllUsersAsync()
        {
            return await _context.Users.ToArrayAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Quotes

        public async Task<List<PoliticianQuote>> GetAllQuotesAsync()
        {
            return await _context.PoliticianQuotes.ToListAsync();
        }

        public async Task<PoliticianQuote?> GetQuoteByIdAsync(int id)
        {
            return await _context.PoliticianQuotes.FirstOrDefaultAsync(q => q.QuoteId == id);
        }

        public async Task UpdateQuoteAsync(PoliticianQuote quote)
        {
            _context.Entry(quote).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Politician

        public async Task<int?> GetAktorIdByTwitterIdAsync(int twitterId)
        {
            return await _context
                .PoliticianTwitterIds.AsNoTracking()
                .Where(p => p.Id == twitterId) // filter by Twitter-ID
                .Select(p => p.AktorId)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}
