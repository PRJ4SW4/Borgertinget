using System; // For DateOnly
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Enums;
using backend.Models;

namespace backend.Interfaces.Repositories
{
    public interface IDailySelectionRepository
    {
        Task<DailySelection?> GetByDateAndModeAsync(
            DateOnly date,
            GamemodeTypes gameMode,
            bool includeAktor = false
        );
        Task<bool> ExistsForDateAsync(DateOnly date);
        Task AddManyAsync(IEnumerable<DailySelection> selections);

        // SaveChangesAsync håndteres typisk af UnitOfWork eller centralt i service
        Task DeleteByDateAsync(DateOnly date);
    }
}
