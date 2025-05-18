using System; // For DateOnly
using System.Threading.Tasks;
using backend.Enums;
using backend.Models;
using backend.Models.Politicians;

namespace backend.Interfaces.Repositories
{
    public interface IGamemodeTrackerRepository
    {
        Task<GamemodeTracker?> FindByAktorAndModeAsync(int aktorId, GamemodeTypes gameMode);
        Task AddAsync(GamemodeTracker tracker);
        void Update(GamemodeTracker tracker); // Update er ofte synkron i EF Core
        Task UpdateOrCreateForAktorAsync(
            Aktor aktor,
            GamemodeTypes gameMode,
            DateOnly selectionDate
        );
        // SaveChangesAsync håndteres typisk af UnitOfWork eller centralt i service
    }
}
