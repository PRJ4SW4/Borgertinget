using backend.Models;
using backend.DTO;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class DailySelectionService : IDailySelectionService
    {
        #region Fields and Constructor

        private readonly DataContext _context;
        private readonly ILogger<DailySelectionService> _logger;
        private readonly Random _random = new Random();

        public DailySelectionService(DataContext context, ILogger<DailySelectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #endregion // Fields and Constructor

        // --- Andre metoder (CalculateAge, GetSelectedPoliticianIdAsync, GetAllPoliticiansForGuessingAsync, GetQuoteOfTheDayAsync, GetPhotoOfTheDayAsync, ProcessGuessAsync) forbliver UÆNDREDE fra forrige version ---
        #region Helper Methods (CalculateAge, GetSelectedPoliticianIdAsync)
        private int CalculateAge(DateOnly dateOfBirth, DateOnly referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.DayOfYear < dateOfBirth.DayOfYear) { age--; }
            return Math.Max(0, age);
        }
        private async Task<int> GetSelectedPoliticianIdAsync(GamemodeTypes gameMode, DateOnly today)
        {
             var selection = await _context.DailySelections
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);
            if (selection == null) { throw new KeyNotFoundException($"Ingen dagens politiker fundet for {gameMode} d. {today}."); }
            return selection.SelectedPolitikerID;
        }
        #endregion Helper Methods

        #region API Methods - Data Retrieval (GetAllPoliticiansForGuessingAsync, GetQuoteOfTheDayAsync, GetPhotoOfTheDayAsync)
        public async Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync()
        {
             return await _context.FakePolitikere
                    .OrderBy(p => p.PolitikerNavn)
                    .Select(p => new PoliticianSummaryDto { Id = p.Id, Name = p.PolitikerNavn })
                    .ToListAsync();
        }
        public async Task<QuoteDto> GetQuoteOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             var selection = await _context.DailySelections
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == GamemodeTypes.Citat);
            if (selection == null) { throw new KeyNotFoundException($"Ingen dagens politiker/citat fundet for Citat-mode d. {today}."); }
            if (string.IsNullOrEmpty(selection.SelectedQuoteText)) { throw new InvalidOperationException($"Intet specifikt citat blev gemt for Citat-mode d. {today}."); }
            return new QuoteDto { QuoteText = selection.SelectedQuoteText };
        }
        public async Task<PhotoDto> GetPhotoOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             int politicianId = await GetSelectedPoliticianIdAsync(GamemodeTypes.Foto, today);
             var photoPolitician = await _context.FakePolitikere
                                             .Select(p => new { p.Id, p.Portræt })
                                             .FirstOrDefaultAsync(p => p.Id == politicianId);
            if (photoPolitician == null) { throw new KeyNotFoundException($"Politiker med ID {politicianId} for dagens foto blev ikke fundet."); }
             if (photoPolitician.Portræt == null || photoPolitician.Portræt.Length == 0) { throw new InvalidOperationException($"Politiker med ID {politicianId} mangler portræt data."); }
            return new PhotoDto { PortraitBase64 = Convert.ToBase64String(photoPolitician.Portræt) };
        }
        #endregion // API Methods - Data Retrieval

        #region API Methods - Guess Processing (ProcessGuessAsync)
         public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            int targetPoliticianId = await GetSelectedPoliticianIdAsync(guessDto.GameMode, today);
            var politicians = await _context.FakePolitikere
                .Include(p => p.FakeParti)
                .Where(p => p.Id == targetPoliticianId || p.Id == guessDto.GuessedPoliticianId)
                .ToListAsync();
            var targetPolitician = politicians.FirstOrDefault(p => p.Id == targetPoliticianId);
            var guessedPolitician = politicians.FirstOrDefault(p => p.Id == guessDto.GuessedPoliticianId);
            if (targetPolitician == null) throw new KeyNotFoundException($"Dagens politiker ({targetPoliticianId}) for {guessDto.GameMode} findes ikke i databasen.");
            if (guessedPolitician == null) throw new KeyNotFoundException($"Den gættede politiker ({guessDto.GuessedPoliticianId}) findes ikke i databasen.");
            int targetAge = CalculateAge(DateOnly.FromDateTime(targetPolitician.DateOfBirth), today);
            int guessedAge = CalculateAge(DateOnly.FromDateTime(guessedPolitician.DateOfBirth), today);
            var result = new GuessResultDto { /* ... mapping ... */ };
            if (guessDto.GameMode == GamemodeTypes.Klassisk) { result.Feedback = new Dictionary<string, FeedbackType>(); /* ... feedback logic ... */ }
            return result;
        }
        #endregion // API Methods - Guess Processing

        #region Daily Job Method (SelectAndSaveDailyPoliticiansAsync - UPDATED)

        // --- METODE TIL DAGLIGT JOB: Vælg og gem dagens politikere/citat ---
        public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date)
        {
            _logger.LogInformation("Starting daily selection process for {Date}", date);

            bool alreadyExists = await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
            if (alreadyExists)
            {
                _logger.LogWarning("Daily selections already exist for {Date}. Skipping generation.", date);
                return;
            }

            // RETTET: Inkluder GameTrackings for at kunne beregne vægte
            var allPoliticians = await _context.FakePolitikere
                                       .Include(p => p.Quotes)
                                       .Include(p => p.GameTrackings) // <-- TILFØJET INCLUDE
                                       .ToListAsync();

            if (!allPoliticians.Any())
            {
                _logger.LogError("No politicians found in the database. Cannot select daily politicians.");
                return;
            }

            // --- Vælg for Classic ---
            // Beregn vægte for ALLE politikere for Klassisk mode
            var classicCandidates = allPoliticians
                .Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Klassisk, date)))
                .ToList();
            var classicPolitician = SelectWeightedRandom(classicCandidates);
            if (classicPolitician == null) { _logger.LogError("Could not select classic politician."); return; } // Bør ikke ske hvis der er kandidater


            // --- Vælg for Citat ---
            FakePolitiker? quotePolitician = null;
            PoliticianQuote? selectedQuote = null;
            var quoteCandidatesRaw = allPoliticians.Where(p => p.Quotes != null && p.Quotes.Any()).ToList();
            if (!quoteCandidatesRaw.Any())
            {
                _logger.LogWarning("No politicians with quotes found. Selecting random politician for Quote mode fallback.");
                // Fallback: Vægtet valg blandt ALLE politikere for Citat mode
                 var fallbackQuoteCandidates = allPoliticians
                    .Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Citat, date)))
                    .ToList();
                quotePolitician = SelectWeightedRandom(fallbackQuoteCandidates);
                // selectedQuote forbliver null i fallback
            }
            else
            {
                 // Beregn vægte for KUN quote kandidaterne for Citat mode
                 var quoteCandidatesWeighted = quoteCandidatesRaw
                    .Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Citat, date)))
                    .ToList();
                 quotePolitician = SelectWeightedRandom(quoteCandidatesWeighted);
                 if (quotePolitician != null && quotePolitician.Quotes.Any()) // Sikkerhedstjek
                 {
                    selectedQuote = quotePolitician.Quotes.ElementAt(_random.Next(quotePolitician.Quotes.Count));
                 }
                 else if (quotePolitician != null) // Hvis den valgte af en grund ikke havde quotes alligevel
                 {
                     _logger.LogWarning("Selected quote politician {PoliticianId} had no quotes after all. No specific quote selected.", quotePolitician.Id);
                 }
            }
            if (quotePolitician == null) { _logger.LogError("Could not select quote politician."); return; } // Bør ikke ske


            // --- Vælg for Foto ---
             FakePolitiker? photoPolitician = null;
             var photoCandidatesRaw = allPoliticians.Where(p => p.Portræt != null && p.Portræt.Length > 0).ToList();
             if (!photoCandidatesRaw.Any())
             {
                  _logger.LogWarning("No politicians with portraits found. Selecting random politician for Photo mode fallback.");
                  // Fallback: Vægtet valg blandt ALLE politikere for Foto mode
                  var fallbackPhotoCandidates = allPoliticians
                    .Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Foto, date)))
                    .ToList();
                 photoPolitician = SelectWeightedRandom(fallbackPhotoCandidates);
             }
             else
             {
                  // Beregn vægte for KUN foto kandidaterne for Foto mode
                  var photoCandidatesWeighted = photoCandidatesRaw
                    .Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Foto, date)))
                    .ToList();
                  photoPolitician = SelectWeightedRandom(photoCandidatesWeighted);
             }
             if (photoPolitician == null) { _logger.LogError("Could not select photo politician."); return; } // Bør ikke ske


            // --- Gem Valgene ---
            var dailySelections = new List<DailySelection>
              {
                  new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Klassisk, SelectedPolitikerID = classicPolitician.Id },
                  new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Citat, SelectedPolitikerID = quotePolitician.Id, SelectedQuoteText = selectedQuote?.QuoteText },
                  new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Foto, SelectedPolitikerID = photoPolitician.Id }
              };
            _context.DailySelections.AddRange(dailySelections); // Tilføj til context


            // --- OPDATER TRACKER DATA ---
            await UpdateTrackerAsync(classicPolitician, GamemodeTypes.Klassisk, date);
            await UpdateTrackerAsync(quotePolitician, GamemodeTypes.Citat, date);
            await UpdateTrackerAsync(photoPolitician, GamemodeTypes.Foto, date);
            // --------------------------


            // Gem BÅDE DailySelections OG Tracker opdateringer/tilføjelser
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully selected and saved daily politicians for {Date}. Classic: {ClassicId}, Quote: {QuoteId}, Photo: {PhotoId}",
               date, classicPolitician.Id, quotePolitician.Id, photoPolitician.Id);
        }

        #endregion // Daily Job Method

        #region Weighted Selection Helpers

        // Beregner vægten for en politiker for en given gamemode og dato
        private int CalculateSelectionWeight(FakePolitiker politician, GamemodeTypes gameMode, DateOnly currentDate)
        {
            const int maxWeight = 365 * 2; // Max vægt (f.eks. 2 år i dage) - juster efter behov
            const int baseWeight = 1; // Minimumsvægt, så alle har en chance

            // Find den relevante tracker entry fra den INKLUDEREDE collection (hurtigere end DB kald)
            var tracker = politician.GameTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);

            if (tracker == null || tracker.LastSelectedDate == null)
            {
                // Aldrig valgt før i denne mode, eller ingen tracker data -> højeste vægt
                return maxWeight;
            }
            else
            {
                // Beregn antal dage siden sidst valgt
                int daysSinceLastSelected = (currentDate.DayNumber - tracker.LastSelectedDate.Value.DayNumber);

                // Sørg for at vægten ikke bliver negativ eller nul (minimum baseWeight)
                // Og cap ved maxWeight
                return Math.Max(baseWeight, Math.Min(maxWeight, daysSinceLastSelected + baseWeight));
            }
        }

        // Vælger en tilfældig politiker fra en liste baseret på vægte
        private FakePolitiker? SelectWeightedRandom(List<(FakePolitiker politician, int weight)> weightedCandidates)
        {
            if (weightedCandidates == null || !weightedCandidates.Any())
            {
                return null; // Ingen kandidater
            }

            // Filtrer kandidater med ugyldig vægt fra (bør ikke ske med CalculateSelectionWeight)
            var validCandidates = weightedCandidates.Where(c => c.weight > 0).ToList();
             if (!validCandidates.Any())
            {
                 // Hvis alle har vægt 0 (f.eks. lige valgt i går), vælg tilfældigt blandt dem
                 _logger.LogWarning("All candidates had zero or negative weight. Selecting randomly from original list.");
                 if (!weightedCandidates.Any()) return null; // Stadig ingen?
                 return weightedCandidates[_random.Next(weightedCandidates.Count)].politician;
            }


            long totalWeight = validCandidates.Sum(c => (long)c.weight); // Brug long for at undgå overflow
            if (totalWeight <= 0) {
                 _logger.LogWarning("Total weight is zero or negative. Selecting randomly from valid candidates.");
                  return validCandidates[_random.Next(validCandidates.Count)].politician;
            }


            // Generer et tilfældigt tal op til den totale vægt (OBS: NextDouble() er 0.0 til < 1.0)
            double randomValue = _random.NextDouble() * totalWeight;

            long cumulativeWeight = 0;
            foreach (var candidate in validCandidates)
            {
                cumulativeWeight += candidate.weight;
                if (randomValue < cumulativeWeight)
                {
                    return candidate.politician; // Fundet!
                }
            }

            // Skulle teoretisk ikke nå hertil pga. NextDouble's range, men som fallback:
             _logger.LogError("Weighted random selection failed to select an item unexpectedly. Returning last valid candidate.");
            return validCandidates.LastOrDefault().politician;
        }


        // Opdaterer (eller opretter) en tracker entry for en valgt politiker
         private async Task UpdateTrackerAsync(FakePolitiker politician, GamemodeTypes gameMode, DateOnly selectionDate)
         {
             // Prøv at finde en eksisterende tracker direkte i DbContext eller via inkluderet data
             // Da vi inkluderede GameTrackings, kan vi tjekke der først
             var existingTracker = politician.GameTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);

             if (existingTracker != null)
             {
                 // Opdater eksisterende
                 existingTracker.LastSelectedDate = selectionDate;
                 existingTracker.AlgoWeight = null; // Nulstil evt. gemt vægt, da den beregnes dynamisk
                 _context.GameTrackings.Update(existingTracker); // Fortæl EF Core den er opdateret
                 _logger.LogDebug("Updating tracker for Politician {PoliticianId}, Gamemode {Gamemode}", politician.Id, gameMode);
             }
             else
             {
                 // Opret ny tracker hvis ingen fandtes
                 var newTracker = new PolidleGamemodeTracker
                 {
                     PolitikerId = politician.Id,
                     GameMode = gameMode,
                     LastSelectedDate = selectionDate,
                     AlgoWeight = null // Beregnes dynamisk
                 };
                 await _context.GameTrackings.AddAsync(newTracker); // Tilføj ny til context
                 _logger.LogDebug("Adding new tracker for Politician {PoliticianId}, Gamemode {Gamemode}", politician.Id, gameMode);
                 // Tilføj også til in-memory collection hvis nødvendigt for resten af metoden (ikke her)
                 // politician.GameTrackings?.Add(newTracker);
             }
         }


        #endregion // Weighted Selection Helpers
    }
}