// Fil: DailySelectionService.cs
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

        #region Interface Implementation: IDailySelectionService

        // GetOrSelectDailyPoliticianAsync (Inkluderer parti)
        public async Task<FakePolitiker?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            _logger.LogDebug("Attempting to get daily politician for {GameMode} on {Date}", gameMode, today);

            var existingSelection = await _context.DailySelections
                                            .Include(ds => ds.SelectedPolitiker)
                                                .ThenInclude(p => p.FakeParti)  // VIGTIGT: Inkluder parti
                                            .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);

            if (existingSelection?.SelectedPolitiker != null)
            {
                _logger.LogInformation("Found existing daily selection for {GameMode} on {Date}: PolitikerId {PolitikerId}", gameMode, today, existingSelection.SelectedPolitikerID);
                 // Sikkerhedstjek for Include
                 if (existingSelection.SelectedPolitiker.FakeParti == null && existingSelection.SelectedPolitiker.PartiId > 0)
                {
                     _logger.LogWarning("Party not included for existing selection's politician {PolitikerId}. Attempting manual load.", existingSelection.SelectedPolitikerID);
                     existingSelection.SelectedPolitiker.FakeParti = await _context.FakePartier.FindAsync(existingSelection.SelectedPolitiker.PartiId);
                }
                return existingSelection.SelectedPolitiker;
            }

            _logger.LogInformation("No existing selection found for {GameMode} on {Date}. Performing weighted selection.", gameMode, today);
            try
            {
                // Udfør den vægtede udvælgelse (som nu returnerer politiker med parti)
                var selectedPolitician = await PerformWeightedSelectionAsync(gameMode, today);

                if (selectedPolitician == null)
                {
                    _logger.LogWarning("Weighted selection for {GameMode} returned no politician.", gameMode);
                    return null;
                }

                 // Dobbelttjek parti (burde være loaded af PerformWeightedSelectionAsync nu)
                 if (selectedPolitician.FakeParti == null && selectedPolitician.PartiId > 0)
                {
                     _logger.LogError("Party was unexpectedly null after PerformWeightedSelectionAsync for politician {PolitikerId}. Attempting manual load.", selectedPolitician.Id);
                     selectedPolitician.FakeParti = await _context.FakePartier.FindAsync(selectedPolitician.PartiId);
                      if (selectedPolitician.FakeParti == null) {
                        _logger.LogError("Could not find party with ID {PartiId} for selected politician {PolitikerId} after manual load.", selectedPolitician.PartiId, selectedPolitician.Id);
                        return null; // Kan ikke fortsætte uden parti
                     }
                }

                // Gem det nye valg og opdater tracking i en transaktion
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newDailySelection = new DailySelection
                    {
                        SelectionDate = today,
                        GameMode = gameMode,
                        SelectedPolitikerID = selectedPolitician.Id,
                    };
                    _context.DailySelections.Add(newDailySelection);

                    await UpdateTrackerAsync(selectedPolitician, gameMode, today); // Opdater/opret tracker

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully selected and saved PolitikerId {PolitikerId} for {GameMode} on {Date}", selectedPolitician.Id, gameMode, today);
                    return selectedPolitician; // Returner den valgte politiker (med parti)
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving daily selection or updating tracking for {GameMode}.", gameMode);
                    throw; // Kast fejlen videre
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during weighted selection process call for {GameMode}.", gameMode);
                throw; // Kast fejlen videre
            }
        }

        // GetAllPoliticiansForGuessingAsync (Med server-side search)
        public async Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync(string? search = null)
        {
            _logger.LogInformation("Fetching politicians for guessing with search term: '{SearchTerm}'", search ?? "<null>");
            var query = _context.FakePolitikere.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTermLower = search.ToLower().Trim();
                // Brug EF.Functions.ILike for case-insensitive på PostgreSQL, ellers ToLower()
                // query = query.Where(p => EF.Functions.ILike(p.PolitikerNavn, $"%{searchTermLower}%"));
                query = query.Where(p => p.PolitikerNavn.ToLower().Contains(searchTermLower)); // Standard fallback
            }

            int maxResults = 10; // Begræns antal søgeresultater
            var politicians = await query
                .OrderBy(p => p.PolitikerNavn)
                .Select(p => new PoliticianSummaryDto
                {
                    Id = p.Id,
                    PolitikerNavn = p.PolitikerNavn,
                    Portraet = p.Portræt // Antager Portræt (med æ) i Model, Portraet (uden æ) i DTO
                })
                .Take(maxResults)
                .ToListAsync();

            _logger.LogInformation("Returning {Count} politician summaries for search term: '{SearchTerm}'", politicians.Count, search ?? "<null>");
            return politicians;
        }

        // ProcessGuessAsync (Opdateret til at bruge CalculateAge med DateTime->DateOnly konvertering)
        public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
        {
             _logger.LogInformation("Processing guess for GameMode {GameMode}, GuessedId {GuessedId}", guessDto.GameMode, guessDto.GuessedPoliticianId);

             // 1. Hent korrekt politiker (m/ parti)
             var correctPolitician = await GetOrSelectDailyPoliticianAsync(guessDto.GameMode);
             if (correctPolitician == null) { throw new KeyNotFoundException($"Ingen dagens politiker fundet for spiltype {guessDto.GameMode}."); }

             // 2. Hent gættet politiker (m/ parti)
             var guessedPolitician = await _context.FakePolitikere
                                             .Include(p => p.FakeParti)
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.Id == guessDto.GuessedPoliticianId);
             if (guessedPolitician == null) { throw new KeyNotFoundException($"Den gættede politiker med ID {guessDto.GuessedPoliticianId} blev ikke fundet."); }

             // *** BEREGN ALDRE MED KONVERTERING FRA DateTime ***
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             // Konverter DateTime DateOfBirth til DateOnly før kald til CalculateAge
             int correctAge = CalculateAge(DateOnly.FromDateTime(correctPolitician.DateOfBirth), today);
             int guessedAge = CalculateAge(DateOnly.FromDateTime(guessedPolitician.DateOfBirth), today);
             // ****************************************************

             // 3. Byg resultat DTO'en
             var result = new GuessResultDto
             {
                 IsCorrectGuess = correctPolitician.Id == guessedPolitician.Id,
                 Feedback = new Dictionary<string, FeedbackType>(),
                 GuessedPolitician = new GuessedPoliticianDetailsDto
                 {
                      Id = guessedPolitician.Id,
                      PolitikerNavn = guessedPolitician.PolitikerNavn,
                      PartiNavn = guessedPolitician.FakeParti?.PartiNavn ?? "Ukendt Parti",
                      Age = guessedAge, // Brug beregnet alder
                      Køn = guessedPolitician.Køn,
                      Uddannelse = guessedPolitician.Uddannelse,
                      Region = guessedPolitician.Region,
                      Portraet = guessedPolitician.Portræt ?? Array.Empty<byte>()
                 }
             };

             // 4. Udfør sammenligninger (kun hvis Klassisk mode)
             if (guessDto.GameMode == GamemodeTypes.Klassisk)
             {
                 result.Feedback["Navn"] = result.IsCorrectGuess ? FeedbackType.Korrekt : FeedbackType.Forkert;
                 result.Feedback["Parti"] = correctPolitician.PartiId == guessedPolitician.PartiId ? FeedbackType.Korrekt : FeedbackType.Forkert;

                 // Sammenlign beregnede aldre
                 if (correctAge == guessedAge) result.Feedback["Alder"] = FeedbackType.Korrekt;
                 else if (correctAge > guessedAge) result.Feedback["Alder"] = FeedbackType.Højere;
                 else result.Feedback["Alder"] = FeedbackType.Lavere;

                 result.Feedback["Region"] = string.Equals(correctPolitician.Region, guessedPolitician.Region, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
                 result.Feedback["Køn"] = string.Equals(correctPolitician.Køn, guessedPolitician.Køn, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
                 result.Feedback["Uddannelse"] = string.Equals(correctPolitician.Uddannelse, guessedPolitician.Uddannelse, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
             }
             // Tilføj evt. simpel feedback for andre modes hvis nødvendigt

             _logger.LogInformation("Guess result calculated for GuessedId {GuessedId}: IsCorrect={IsCorrect}, FeedbackItems={FeedbackCount}",
                 guessDto.GuessedPoliticianId, result.IsCorrectGuess, result.Feedback.Count);

             return result;
        }

        // GetQuoteOfTheDayAsync
        public async Task<QuoteDto> GetQuoteOfTheDayAsync()
        {
              DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
              var selection = await _context.DailySelections.AsNoTracking()
                  .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == GamemodeTypes.Citat);
             if (selection == null) { throw new KeyNotFoundException($"Ingen dagens citat fundet for {today}."); }
             if (string.IsNullOrEmpty(selection.SelectedQuoteText)) { throw new InvalidOperationException($"Intet citat gemt for {today}."); }
             return new QuoteDto { QuoteText = selection.SelectedQuoteText };
        }

        // GetPhotoOfTheDayAsync
        public async Task<PhotoDto> GetPhotoOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             int politicianId = await GetSelectedPoliticianIdAsync(GamemodeTypes.Foto, today);
             var photoData = await _context.FakePolitikere
                                     .Where(p => p.Id == politicianId)
                                     .Select(p => p.Portræt)
                                     .FirstOrDefaultAsync();
             if (photoData == null) { throw new KeyNotFoundException($"Politiker ({politicianId}) fundet i DailySelection, men ikke i FakePolitikere, eller Portræt er null."); }
             if (photoData.Length == 0) { throw new InvalidOperationException($"Politiker ({politicianId}) har tomme portræt data."); }
             return new PhotoDto { PortraitBase64 = Convert.ToBase64String(photoData) };
        }

        // SelectAndSaveDailyPoliticiansAsync (Job metode)
         public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date)
         {
              _logger.LogInformation("Starting daily selection process for {Date}", date);
              bool alreadyExists = await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
              if (alreadyExists) { _logger.LogWarning("Daily selections already exist for {Date}. Skipping generation.", date); return; }

              // Antager at FakePolitiker har en navigation property 'Quotes' af typen ICollection<PoliticianQuote>
              // og at PoliticianQuote har en string property 'QuoteText'
              var allPoliticians = await _context.FakePolitikere
                                              .Include(p => p.Quotes)
                                              .Include(p => p.GameTrackings)
                                              .ToListAsync();
              if (!allPoliticians.Any()) { _logger.LogError("No politicians found."); return; }

              // Vælg Classic
              var classicCandidates = allPoliticians.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Klassisk, date))).ToList();
              var classicPolitician = SelectWeightedRandom(classicCandidates);
              if (classicPolitician == null) { _logger.LogError("Could not select classic politician."); return; }

              // Vælg Citat (med fallback)
              FakePolitiker? quotePolitician = null; PoliticianQuote? selectedQuote = null;
              var quoteCandidatesRaw = allPoliticians.Where(p => p.Quotes != null && p.Quotes.Any()).ToList();
              if (!quoteCandidatesRaw.Any()) {
                   _logger.LogWarning("No politicians with quotes found. Selecting random politician for Quote mode fallback.");
                   var fallbackQuoteCandidates = allPoliticians.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Citat, date))).ToList();
                   quotePolitician = SelectWeightedRandom(fallbackQuoteCandidates);
               } else {
                   var quoteCandidatesWeighted = quoteCandidatesRaw.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Citat, date))).ToList();
                   quotePolitician = SelectWeightedRandom(quoteCandidatesWeighted);
                   if (quotePolitician?.Quotes?.Any() ?? false) { selectedQuote = quotePolitician.Quotes.ElementAt(_random.Next(quotePolitician.Quotes.Count)); }
               }
               if (quotePolitician == null) { _logger.LogError("Could not select quote politician."); return; }

               // Vælg Foto (med fallback)
                FakePolitiker? photoPolitician = null;
                var photoCandidatesRaw = allPoliticians.Where(p => p.Portræt != null && p.Portræt.Length > 0).ToList();
                 if (!photoCandidatesRaw.Any()) {
                    _logger.LogWarning("No politicians with portraits found. Selecting random politician for Photo mode fallback.");
                    var fallbackPhotoCandidates = allPoliticians.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Foto, date))).ToList();
                    photoPolitician = SelectWeightedRandom(fallbackPhotoCandidates);
                } else {
                     var photoCandidatesWeighted = photoCandidatesRaw.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Foto, date))).ToList();
                     photoPolitician = SelectWeightedRandom(photoCandidatesWeighted);
                 }
                 if (photoPolitician == null) { _logger.LogError("Could not select photo politician."); return; }

              // Gem Valg
              var dailySelections = new List<DailySelection> {
                   new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Klassisk, SelectedPolitikerID = classicPolitician.Id },
                   new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Citat, SelectedPolitikerID = quotePolitician.Id, SelectedQuoteText = selectedQuote?.QuoteText }, // Antager PoliticianQuote model
                   new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Foto, SelectedPolitikerID = photoPolitician.Id }
               };
              _context.DailySelections.AddRange(dailySelections);

              // Opdater Trackers
              await UpdateTrackerAsync(classicPolitician, GamemodeTypes.Klassisk, date);
              await UpdateTrackerAsync(quotePolitician, GamemodeTypes.Citat, date);
              await UpdateTrackerAsync(photoPolitician, GamemodeTypes.Foto, date);

              await _context.SaveChangesAsync();
              _logger.LogInformation("Successfully selected and saved daily politicians for {Date}. Classic: {ClassicId}, Quote: {QuoteId}, Photo: {PhotoId}", date, classicPolitician.Id, quotePolitician.Id, photoPolitician.Id);
         }

        #endregion // Interface Implementation

        #region Helper Methods

        // CalculateAge (uændret - forventer DateOnly)
        private int CalculateAge(DateOnly dateOfBirth, DateOnly referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.DayOfYear < dateOfBirth.DayOfYear) { age--; }
            return Math.Max(0, age);
        }

        // GetSelectedPoliticianIdAsync (uændret)
         private async Task<int> GetSelectedPoliticianIdAsync(GamemodeTypes gameMode, DateOnly today)
         {
             var selection = await _context.DailySelections.AsNoTracking().FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);
             if (selection == null) { throw new KeyNotFoundException($"Ingen dagens politiker fundet for {gameMode} d. {today}."); }
             return selection.SelectedPolitikerID;
         }
        #endregion Helper Methods

        #region Weighted Selection Helpers

        // PerformWeightedSelectionAsync (med rettet Include/Select)
        private async Task<FakePolitiker?> PerformWeightedSelectionAsync(GamemodeTypes gameMode, DateOnly today)
        {
             const int maxWeightDays = 365 * 2; const int defaultWeight = maxWeightDays + 1;
             _logger.LogDebug("Performing weighted selection for {GameMode} on {Date}", gameMode, today);

             var candidates = await _context.FakePolitikere
                 .Include(p => p.FakeParti) // Inkluder parti
                 .Include(p => p.GameTrackings.Where(gt => gt.GameMode == gameMode)) // Inkluder relevant tracking
                 // .Where(p => p.IsActive) // Evt. filter
                 .Select(p => new { Politician = p }) // Projektér EFTER includes
                 .ToListAsync();

             if (!candidates.Any()) { _logger.LogWarning("No candidates found for weighted selection..."); return null; }

             var weightedCandidates = candidates.Select(c => {
                 var tracker = c.Politician.GameTrackings.FirstOrDefault(); // Bør kun være én
                 int daysSinceLast = tracker?.LastSelectedDate.HasValue ?? false ? today.DayNumber - tracker.LastSelectedDate.Value.DayNumber : defaultWeight;
                 int weight = Math.Max(1, Math.Min(daysSinceLast, maxWeightDays));
                 return new { c.Politician, Weight = weight };
             }).ToList();

             return SelectWeightedRandom(weightedCandidates.Select(wc => (wc.Politician, wc.Weight)).ToList());
        }

        // CalculateSelectionWeight (som før)
         private int CalculateSelectionWeight(FakePolitiker politician, GamemodeTypes gameMode, DateOnly currentDate)
         {
              const int maxWeight = 365 * 2; const int baseWeight = 1;
              var tracker = politician.GameTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);
              if (tracker == null || tracker.LastSelectedDate == null) { return maxWeight; }
              else { int days = (currentDate.DayNumber - tracker.LastSelectedDate.Value.DayNumber); return Math.Max(baseWeight, Math.Min(maxWeight, days + baseWeight)); }
         }

        // SelectWeightedRandom (som før)
         private FakePolitiker? SelectWeightedRandom(List<(FakePolitiker politician, int weight)> weightedCandidates)
         {
              if (weightedCandidates == null || !weightedCandidates.Any()) return null;
              var validCandidates = weightedCandidates.Where(c => c.weight > 0).ToList();
               if (!validCandidates.Any()) { /* fallback */ if (!weightedCandidates.Any()) return null; return weightedCandidates[_random.Next(weightedCandidates.Count)].politician;}
               long totalWeight = validCandidates.Sum(c => (long)c.weight);
               if (totalWeight <= 0) { /* fallback */ return validCandidates[_random.Next(validCandidates.Count)].politician; }
               double randomValue = _random.NextDouble() * totalWeight; long cumulativeWeight = 0;
               foreach (var candidate in validCandidates) { cumulativeWeight += candidate.weight; if (randomValue < cumulativeWeight) return candidate.politician; }
               _logger.LogError("Weighted random selection failed unexpectedly..."); return validCandidates.LastOrDefault().politician; // Fallback
         }

        // UpdateTrackerAsync (som før)
         private async Task UpdateTrackerAsync(FakePolitiker politician, GamemodeTypes gameMode, DateOnly selectionDate)
         {
             var existingTracker = politician.GameTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);
             if (existingTracker == null) { existingTracker = await _context.GameTrackings.FirstOrDefaultAsync(gt => gt.PolitikerId == politician.Id && gt.GameMode == gameMode); }
              if (existingTracker != null) { existingTracker.LastSelectedDate = selectionDate; existingTracker.AlgoWeight = null; _context.GameTrackings.Update(existingTracker); }
              else { var nt = new PolidleGamemodeTracker { PolitikerId = politician.Id, GameMode = gameMode, LastSelectedDate = selectionDate }; await _context.GameTrackings.AddAsync(nt); }
             // Bemærk: SaveChanges() kaldes samlet i SelectAndSaveDailyPoliticiansAsync
         }

        #endregion // Weighted Selection Helpers
    }
}