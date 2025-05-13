using backend.Data;
using backend.Interfaces.Repositories;
using backend.Models;
using backend.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Tilføjet for logging
using System;
using System.Threading.Tasks;

namespace backend.Persistence.Repositories
{
    public class GamemodeTrackerRepository : IGamemodeTrackerRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<GamemodeTrackerRepository> _logger; // Tilføjet logger


        public GamemodeTrackerRepository(DataContext context, ILogger<GamemodeTrackerRepository> logger) // Tilføjet logger
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GamemodeTracker?> FindByAktorAndModeAsync(int aktorId, GamemodeTypes gameMode)
        {
            return await _context.GamemodeTrackers
                .FirstOrDefaultAsync(gt => gt.PolitikerId  == aktorId && gt.GameMode == gameMode);
        }

        public async Task AddAsync(GamemodeTracker tracker)
        {
            await _context.GamemodeTrackers.AddAsync(tracker);
             // SaveChangesAsync kaldes centralt
        }

        public void Update(GamemodeTracker tracker)
        {
             _context.GamemodeTrackers.Update(tracker);
             // SaveChangesAsync kaldes centralt
        }

         public async Task UpdateOrCreateForAktorAsync(Aktor aktor, GamemodeTypes gameMode, DateOnly selectionDate)
         {
             // Forsøg at finde eksisterende tracker via navigation property først (hvis loaded)
             var existingTracker = aktor.GamemodeTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);

             if (existingTracker == null)
             {
                 // Hvis ikke loaded, prøv at finde i DB
                  existingTracker = await FindByAktorAndModeAsync(aktor.Id, gameMode);
             }

             if (existingTracker != null)
             {
                 // Opdater
                 existingTracker.LastSelectedDate = selectionDate;
                 existingTracker.AlgoWeight = null; // Nulstil evt. gammel vægt
                 Update(existingTracker);
                 _logger.LogDebug("Updated GamemodeTracker for Aktor {AktorId}, Gamemode {Gamemode}, Date {Date}", aktor.Id, gameMode, selectionDate);
             }
             else
             {
                 // Opret ny
                  var newTracker = new GamemodeTracker
                  {
                      PolitikerId  = aktor.Id,
                      GameMode = gameMode,
                      LastSelectedDate = selectionDate,
                      AlgoWeight = null
                  };
                  await AddAsync(newTracker);
                   _logger.LogDebug("Created new GamemodeTracker for Aktor {AktorId}, Gamemode {Gamemode}, Date {Date}", aktor.Id, gameMode, selectionDate);
             }
         }
    }
}