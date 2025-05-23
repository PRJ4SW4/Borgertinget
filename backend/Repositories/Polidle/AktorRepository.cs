using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Interfaces.Repositories;
using backend.Models.Politicians;
using Microsoft.EntityFrameworkCore;

namespace backend.Persistence.Repositories
{
    public class AktorRepository : IAktorRepository
    {
        private readonly DataContext _context;

        public AktorRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Aktor?> GetByIdAsync(int id, bool includeParty = false)
        {
            var query = _context.Aktor.AsQueryable();
            //if (includeParty) query = query.Include(a => a.Party); // Tilpas til din Party relation
            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Aktor>> GetAllForSummaryAsync(
            string? search = null,
            int maxResults = 15
        )
        {
            var query = _context.Aktor.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTermLower = search.ToLower().Trim();
                query = query.Where(p =>
                    p.navn != null && p.navn.ToLower().Contains(searchTermLower)
                );
            }
            return await query.OrderBy(p => p.navn).Take(maxResults).ToListAsync();
        }

        public async Task<List<Aktor>> GetAllWithDetailsForSelectionAsync()
        {
            // Inkluder alt nødvendigt for *alle* gamemodes' udvælgelse + tracker opdatering
            return await _context
                .Aktor.Include(a => a.Quotes)
                .Include(a => a.GamemodeTrackings)
                // .Include(a => a.Party) // Hvis Party relation bruges direkte
                .Where(a => a.navn != null) // Eksempel på filter
                .ToListAsync();
        }
    }
}
