using backend.Models;
using backend.Enums;
using System; // For DateOnly
using System.Threading.Tasks;

namespace backend.Interfaces.Repositories
{
    public interface IGamemodeTrackerRepository
    {
        Task<GamemodeTracker?> FindByAktorAndModeAsync(int aktorId, GamemodeTypes gameMode);
        Task AddAsync(GamemodeTracker tracker);
        void Update(GamemodeTracker tracker); // Update er ofte synkron i EF Core
        Task UpdateOrCreateForAktorAsync(Aktor aktor, GamemodeTypes gameMode, DateOnly selectionDate);
         // SaveChangesAsync håndteres typisk af UnitOfWork eller centralt i service
    }
}