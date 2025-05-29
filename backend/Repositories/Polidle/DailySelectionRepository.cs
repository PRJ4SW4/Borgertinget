using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Enums;
using backend.Interfaces.Repositories;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.PolidleSelection
{
    public class DailySelectionRepository : IDailySelectionRepository
    {
        private readonly DataContext _context;

        public DailySelectionRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<DailySelection?> GetByDateAndModeAsync(
            DateOnly date,
            GamemodeTypes gameMode,
            bool includeAktor = false
        )
        {
            var query = _context.DailySelections.AsQueryable();
            if (includeAktor)
            {
                query = query.Include(ds => ds.SelectedPolitiker);
            }
            return await query.FirstOrDefaultAsync(ds =>
                ds.SelectionDate == date && ds.GameMode == gameMode
            );
        }

        public async Task<bool> ExistsForDateAsync(DateOnly date)
        {
            return await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
        }

        public async Task AddManyAsync(IEnumerable<DailySelection> selections)
        {
            if (selections == null || !selections.Any())
                return;
            await _context.DailySelections.AddRangeAsync(selections);
        }

        public async Task DeleteByDateAsync(DateOnly date)
        {
            var selectionsToDelete = await _context
                .DailySelections.Where(ds => ds.SelectionDate == date)
                .ToListAsync();

            if (selectionsToDelete.Any())
            {
                _context.DailySelections.RemoveRange(selectionsToDelete);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
