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

        Task DeleteByDateAsync(DateOnly date);

        Task<int> SaveChangesAsync();
    }
}
