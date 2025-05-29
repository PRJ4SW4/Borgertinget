using backend.Data;
using backend.Models;
using backend.Models.Flashcards;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class AdministratorRepository : IAdministratorRepository
    {
        private readonly DataContext _context;
        public AdministratorRepository(DataContext context, ILogger<AdministratorRepository> logger)
        {
            _context = context;
        }

        #region Flashcard collections

        public async Task AddFlashcardCollectionAsync(FlashcardCollection collection)
        {
            _context.FlashcardCollections.Add(collection);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFlashcardCollectionAsync(FlashcardCollection collection)
        {
            // Mark Flashcard collection entity as modified so EF Core will update all its properties in the database
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

        public async Task UpdateUserAsync(User user)
        {
            // Mark User entity as modified so EF Core will update all its properties in the database
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
            // Mark quote entity as modified so EF Core will update all its properties in the database
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
