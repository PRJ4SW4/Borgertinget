using System;
using System.Threading.Tasks;
using backend.Data;
using backend.Enums;
using backend.Interfaces.Repositories;
using backend.Models;
using backend.Models.Politicians;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Repositories.PolidleTracker
{
    public class GamemodeTrackerRepository : IGamemodeTrackerRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<GamemodeTrackerRepository> _logger;

        public GamemodeTrackerRepository(
            DataContext context,
            ILogger<GamemodeTrackerRepository> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GamemodeTracker?> FindByAktorAndModeAsync(
            int aktorId,
            GamemodeTypes gameMode
        )
        {
            return await _context.GamemodeTrackers.FirstOrDefaultAsync(gt =>
                gt.PolitikerId == aktorId && gt.GameMode == gameMode
            );
        }

        public async Task AddAsync(GamemodeTracker tracker)
        {
            await _context.GamemodeTrackers.AddAsync(tracker);
        }

        public void Update(GamemodeTracker tracker)
        {
            _context.GamemodeTrackers.Update(tracker);
        }

        public async Task UpdateOrCreateForAktorAsync(
            Aktor aktor,
            GamemodeTypes gameMode,
            DateOnly selectionDate
        )
        {
            var existingTracker = aktor.GamemodeTrackings?.FirstOrDefault(gt =>
                gt.GameMode == gameMode
            );

            if (existingTracker == null)
            {
                existingTracker = await FindByAktorAndModeAsync(aktor.Id, gameMode);
            }

            if (existingTracker != null)
            {
                // Opdater
                existingTracker.LastSelectedDate = selectionDate;
                existingTracker.AlgoWeight = null;
                Update(existingTracker);
                _logger.LogDebug(
                    "Updated GamemodeTracker for Aktor {AktorId}, Gamemode {Gamemode}, Date {Date}",
                    aktor.Id,
                    gameMode,
                    selectionDate
                );
            }
            else
            {
                // Opret ny
                var newTracker = new GamemodeTracker
                {
                    PolitikerId = aktor.Id,
                    GameMode = gameMode,
                    LastSelectedDate = selectionDate,
                    AlgoWeight = null,
                };
                await AddAsync(newTracker);
                _logger.LogDebug(
                    "Created new GamemodeTracker for Aktor {AktorId}, Gamemode {Gamemode}, Date {Date}",
                    aktor.Id,
                    gameMode,
                    selectionDate
                );
            }
        }
    }
}
