using backend.Enums;
using backend.Models;
using backend.Models.Politicians;

namespace backend.Interfaces.Repositories
{
    public interface IGamemodeTrackerRepository
    {
        Task<GamemodeTracker?> FindByAktorAndModeAsync(int aktorId, GamemodeTypes gameMode);
        Task AddAsync(GamemodeTracker tracker);
        void Update(GamemodeTracker tracker);
        Task UpdateOrCreateForAktorAsync(
            Aktor aktor,
            GamemodeTypes gameMode,
            DateOnly selectionDate
        );
    }
}
