using backend.Data;
using backend.Interfaces.Repositories;
using backend.Models;
using backend.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace backend.Persistence.Repositories
{
    public class DailySelectionRepository : IDailySelectionRepository
    {
        private readonly DataContext _context;

        public DailySelectionRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<DailySelection?> GetByDateAndModeAsync(DateOnly date, GamemodeTypes gameMode, bool includeAktor = false)
        {
            var query = _context.DailySelections.AsQueryable();
            if (includeAktor)
            {
                 query = query.Include(ds => ds.SelectedPolitiker);
                     //.ThenInclude(a => a.Party); // Inkluder evt. parti her også
            }
            return await query.FirstOrDefaultAsync(ds => ds.SelectionDate == date && ds.GameMode == gameMode);
        }

        public async Task<bool> ExistsForDateAsync(DateOnly date)
        {
            return await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
        }

        public async Task AddManyAsync(IEnumerable<DailySelection> selections)
        {
             if (selections == null || !selections.Any()) return;
             await _context.DailySelections.AddRangeAsync(selections);
             // SaveChangesAsync kaldes centralt (f.eks. i service eller Unit of Work)
        }
    }
}