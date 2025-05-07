// Fil: DailySelectionService.cs
using backend.Models;
using backend.DTO; // For Polidle DTOs like GuessResultDto, PoliticianSummaryDto etc.
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Make sure you have a using statement for your DataParsingHelpers
using backend.Services; // Or backend.Helpers if you placed DataParsingHelpers there

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

        public async Task<Aktor?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            _logger.LogDebug("Attempting to get daily Aktor for {GameMode} on {Date}", gameMode, today);

            var existingSelectionQuery = _context.DailySelections
                                                .Include(ds => ds.SelectedPolitiker); // Aktor

            // Conditional Include for Party. This depends on your Aktor model having a
            // navigation property to Party (e.g., Aktor.PartyNavigation) and a corresponding FK.
            // If Aktor.Party is just a string property with the party name, this ThenInclude isn't directly applicable
            // to loading a Party *entity* unless Aktor has a FK to the Party table.
            // The current Aktor model seems to have `string? Party` for the name.
            // If you need to compare based on Party *ID*, Aktor should have a PartyId.
            // For now, we'll assume Aktor.Party (string) is used or PartyId is on Aktor.
            // The `.ThenInclude(p => p.MembersOfParty)` from your code implies SelectedPolitiker had a MembersOfParty property.
            // If Aktor is replacing it, Aktor needs a similar link or you adapt.
            // Let's assume you want to load the Party entity if Aktor has a PartyId FK and navigation property 'PartyEntity'.
            // if (existingSelectionQuery.FirstOrDefault()?.SelectedPolitiker.PartyId != null) // Check if PartyId exists if that's your FK
            // {
            //    existingSelectionQuery = existingSelectionQuery.ThenInclude(ds => ds.SelectedPolitiker.PartyEntity);
            // }


            var existingSelection = await existingSelectionQuery
                                        .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);


            if (existingSelection?.SelectedPolitiker != null)
            {
                _logger.LogInformation("Found existing daily selection for {GameMode} on {Date}: AktorId {AktorId}", gameMode, today, existingSelection.SelectedPolitikerID);
                // If Aktor.Party is just a string, this check/load isn't for the Party *entity* directly.
                // The manual load you had `existingSelection.SelectedPolitiker.MembersOfParty = await _context.Party.FindAsync(existingSelection.SelectedPolitiker.partyId);`
                // implies SelectedPolitiker had a 'partyId' and a 'MembersOfParty' navigation property.
                // Ensure your Aktor model has a corresponding 'PartyId' field if this logic is to be preserved.
                // And a navigation property for the party.
                // For now, we'll assume this is handled or not strictly needed for `GetOrSelectDailyPoliticianAsync` if Party name string is enough.
                return existingSelection.SelectedPolitiker;
            }

            _logger.LogInformation("No existing selection for {GameMode} on {Date}. Performing weighted selection.", gameMode, today);
            try
            {
                var selectedPolitician = await PerformWeightedSelectionAsync(gameMode, today);

                if (selectedPolitician == null)
                {
                    _logger.LogWarning("Weighted selection for {GameMode} returned no Aktor.", gameMode);
                    return null;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newDailySelection = new DailySelection
                    {
                        SelectionDate = today,
                        GameMode = gameMode,
                        SelectedPolitikerID = selectedPolitician.Id,
                    };
                     // If Citat mode, also store the selected quote text
                    if (gameMode == GamemodeTypes.Citat && (selectedPolitician.Quotes?.Any() ?? false))
                    {
                        newDailySelection.SelectedQuoteText = selectedPolitician.Quotes.ElementAt(_random.Next(selectedPolitician.Quotes.Count)).QuoteText;
                    }

                    _context.DailySelections.Add(newDailySelection);
                    await UpdateTrackerAsync(selectedPolitician, gameMode, today); // Update/create tracker
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully selected and saved AktorId {AktorId} for {GameMode} on {Date}", selectedPolitician.Id, gameMode, today);
                    return selectedPolitician;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving daily selection or updating tracking for {GameMode}.", gameMode);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during weighted selection process call for {GameMode}.", gameMode);
                throw;
            }
        }

        public async Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync(string? search = null)
        {
            _logger.LogInformation("Fetching Aktors for guessing with search term: '{SearchTerm}'", search ?? "<null>");
            // Assuming Aktor model has typeid, and politicians are typeid == 5
            var query = _context.Aktor.AsNoTracking().Where(a => a.typeid == 5);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTermLower = search.ToLower().Trim();
                query = query.Where(p => (p.navn != null && p.navn.ToLower().Contains(searchTermLower)));
            }

            int maxResults = 10;
            var politicians = await query
                .OrderBy(p => p.navn)
                .Select(p => new PoliticianSummaryDto
                {
                    Id = p.Id,
                    navn = p.navn ?? "Ukendt Navn",
                    Portraet = p.Portraet ?? Array.Empty<byte>() // Directly use the Aktor.Portraet byte array
                })
                .Take(maxResults)
                .ToListAsync();

            _logger.LogInformation("Returning {Count} Aktor summaries for search term: '{SearchTerm}'", politicians.Count, search ?? "<null>");
            return politicians;
        }

        public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
        {
            _logger.LogInformation("Processing guess for GameMode {GameMode}, GuessedAktorId {GuessedId}", guessDto.GameMode, guessDto.GuessedPoliticianId);

            // 1. Hent korrekt Aktor
            var correctAktor = await GetOrSelectDailyPoliticianAsync(guessDto.GameMode);
            if (correctAktor == null)
            {
                _logger.LogError("Could not retrieve correct Aktor for GameMode {GameMode}.", guessDto.GameMode);
                throw new KeyNotFoundException($"Ingen dagens politiker fundet for spiltype {guessDto.GameMode}.");
            }

            // 2. Hent gættet Aktor
            // Include party if Aktor has a PartyId and a navigation property to Party entity
            // For now, assuming Aktor.Party (string name) and Aktor.Constituencies (List<string>) are sufficient
            var guessedAktor = await _context.Aktor
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(a => a.Id == guessDto.GuessedPoliticianId && a.typeid == 5);

            if (guessedAktor == null)
            {
                _logger.LogError("Guessed Aktor with ID {GuessedId} not found or is not typeid 5.", guessDto.GuessedPoliticianId);
                throw new KeyNotFoundException($"Den gættede politiker med ID {guessDto.GuessedPoliticianId} blev ikke fundet.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            DateTime? correctAktorBirthDateTime = DataParsingHelpers.ParseBornStringToDateTime(correctAktor.Born, _logger);
            if (!correctAktorBirthDateTime.HasValue)
            {
                _logger.LogError("Could not parse DateOfBirth for correct Aktor ID {AktorId}. Born string: '{BornString}'", correctAktor.Id, correctAktor.Born);
                throw new InvalidOperationException($"Could not parse birth date for correct Aktor ID {correctAktor.Id}.");
            }
            int correctAge = CalculateAge(DateOnly.FromDateTime(correctAktorBirthDateTime.Value), today);

            DateTime? guessedAktorBirthDateTime = DataParsingHelpers.ParseBornStringToDateTime(guessedAktor.Born, _logger);
            if (!guessedAktorBirthDateTime.HasValue)
            {
                _logger.LogError("Could not parse DateOfBirth for guessed Aktor ID {AktorId}. Born string: '{BornString}'", guessedAktor.Id, guessedAktor.Born);
                throw new InvalidOperationException($"Could not parse birth date for guessed Aktor ID {guessedAktor.Id}.");
            }
            int guessedAge = CalculateAge(DateOnly.FromDateTime(guessedAktorBirthDateTime.Value), today);

            var guessedDetails = new GuessedPoliticianDetailsDto
            {
                Id = guessedAktor.Id,
                navn = guessedAktor.navn ?? "N/A",
                partyName = guessedAktor.Party ?? "Ukendt Parti", // Uses string Party name from Aktor
                Age = guessedAge,
                Sex = guessedAktor.Sex ?? "N/A",
                Uddannelse = DataParsingHelpers.GetFirstEducation(guessedAktor.Educations) ?? "N/A",
                Region = guessedAktor.Constituencies?.FirstOrDefault() ?? "N/A", // First constituency as region
                Portraet = guessedAktor.Portraet ?? Array.Empty<byte>() // Use the byte array
            };

            var result = new GuessResultDto
            {
                IsCorrectGuess = correctAktor.Id == guessedAktor.Id,
                Feedback = new Dictionary<string, FeedbackType>(),
                GuessedPolitician = guessedDetails
            };

            if (guessDto.GameMode == GamemodeTypes.Klassisk)
            {
                result.Feedback["Navn"] = result.IsCorrectGuess ? FeedbackType.Korrekt : FeedbackType.Forkert;
                
                result.Feedback["Parti"] = string.Equals(correctAktor.Party, guessedAktor.Party, StringComparison.OrdinalIgnoreCase) 
                                            ? FeedbackType.Korrekt 
                                            : FeedbackType.Forkert;

                if (correctAge == guessedAge) result.Feedback["Alder"] = FeedbackType.Korrekt;
                else if (correctAge > guessedAge) result.Feedback["Alder"] = FeedbackType.Højere;
                else result.Feedback["Alder"] = FeedbackType.Lavere;

                string? correctRegionString = correctAktor.Constituencies?.FirstOrDefault();
                string? guessedRegionString = guessedAktor.Constituencies?.FirstOrDefault();
                result.Feedback["Region"] = string.Equals(correctRegionString, guessedRegionString, StringComparison.OrdinalIgnoreCase) 
                                            ? FeedbackType.Korrekt 
                                            : FeedbackType.Forkert;
                
                result.Feedback["Køn"] = string.Equals(correctAktor.Sex, guessedAktor.Sex, StringComparison.OrdinalIgnoreCase) 
                                            ? FeedbackType.Korrekt 
                                            : FeedbackType.Forkert;
                
                string? correctFirstEducation = DataParsingHelpers.GetFirstEducation(correctAktor.Educations);
                string? guessedFirstEducation = DataParsingHelpers.GetFirstEducation(guessedAktor.Educations);
                result.Feedback["Uddannelse"] = string.Equals(correctFirstEducation, guessedFirstEducation, StringComparison.OrdinalIgnoreCase) 
                                            ? FeedbackType.Korrekt 
                                            : FeedbackType.Forkert;
            }

            _logger.LogInformation("Guess result for GuessedAktorId {GuessedId}: Correct={IsCorrect}, Feedback Count={FeedbackCount}",
                guessDto.GuessedPoliticianId, result.IsCorrectGuess, result.Feedback.Count);

            return result;
        }

        public async Task<QuoteDto> GetQuoteOfTheDayAsync()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            var selection = await _context.DailySelections
                .AsNoTracking()
                .Include(ds => ds.SelectedPolitiker) // Aktor
                    .ThenInclude(p => p!.Quotes)    // Quotes from Aktor
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == GamemodeTypes.Citat);

            if (selection == null) { throw new KeyNotFoundException($"Ingen dagens citat fundet for {today}."); }
            
            if (string.IsNullOrEmpty(selection.SelectedQuoteText))
            {
                if (selection.SelectedPolitiker?.Quotes != null && selection.SelectedPolitiker.Quotes.Any())
                {
                    _logger.LogWarning("SelectedQuoteText was missing for Citat selection on {Date} for Aktor {AktorId}. Selecting a random quote now.", today, selection.SelectedPolitikerID);
                    return new QuoteDto { QuoteText = selection.SelectedPolitiker.Quotes.ElementAt(_random.Next(selection.SelectedPolitiker.Quotes.Count)).QuoteText };
                }
                throw new InvalidOperationException($"Intet citat gemt eller findes for politikeren (ID: {selection.SelectedPolitikerID}) for {today}.");
            }
            return new QuoteDto { QuoteText = selection.SelectedQuoteText };
        }

        public async Task<PhotoDto> GetPhotoOfTheDayAsync()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            _logger.LogInformation("Attempting to get photo of the day for {Date}", today);

            var selection = await _context.DailySelections
                .Include(ds => ds.SelectedPolitiker) // Aktor
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == GamemodeTypes.Foto);

            if (selection == null || selection.SelectedPolitiker == null)
            {
                _logger.LogError("Daily selection or Aktor not found for Foto mode on {Date}", today);
                // Consider if GetOrSelectDailyPoliticianAsync should be called here as a fallback
                // to ensure a selection is made if the job hasn't run yet for today.
                // For now, throwing as per previous logic if no selection exists.
                throw new KeyNotFoundException($"Ingen dagens fotovalg eller politiker fundet for {today}.");
            }

            // Directly use the Portraet byte array from the Aktor model
            byte[]? portraitData = selection.SelectedPolitiker.Portraet; 

            if (portraitData == null || portraitData.Length == 0)
            {
                // This means the Aktor entity was found, but its Portraet field is empty or null.
                // This should ideally be handled during data import/seeding.
                _logger.LogWarning("Aktor ID {AktorId} for Photo mode has no portrait byte[] data (Aktor.Portraet is null or empty).", selection.SelectedPolitiker.Id);
                // Return an empty DTO or a DTO with a placeholder/error indicator based on frontend requirements.
                return new PhotoDto { PortraitBase64 = string.Empty }; 
            }

            // If we have portraitData, convert to Base64 for the DTO
            try
            {
                return new PhotoDto { PortraitBase64 = Convert.ToBase64String(portraitData) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert portrait data to Base64 for Aktor ID {AktorId}", selection.SelectedPolitiker.Id);
                // Handle conversion error, e.g., return empty or throw
                return new PhotoDto { PortraitBase64 = string.Empty };
            }
        }
        
        public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date)
        {
             _logger.LogInformation("Starting daily selection process for Aktor for {Date}", date);
            bool alreadyExists = await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
            if (alreadyExists) 
            {
                 _logger.LogWarning("Daily selections already exist for {Date}. Skipping generation.", date); 
                 return; 
            }

            var allPoliticians = await _context.Aktor
                                            .Where(a => a.typeid == 5) 
                                            .Include(a => a.Quotes)
                                            .Include(a => a.GameTrackings)
                                            // .Include(a => a.PartyEntity) // If you have a direct navigation to a Party entity
                                            .ToListAsync();

            if (!allPoliticians.Any()) 
            {
                 _logger.LogError("No Aktors (typeid=5) found in the database for daily selection."); 
                 return; 
            }

            Aktor? classicPolitician = SelectWeightedRandom(allPoliticians.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Klassisk, date))).ToList());
            if (classicPolitician == null) { _logger.LogError("Could not select classic Aktor for {Date}.", date); return; }

            Aktor? quotePolitician;
            PoliticianQuote? selectedQuote = null;
            var quoteCandidates = allPoliticians.Where(p => p.Quotes != null && p.Quotes.Any()).ToList();
            if (!quoteCandidates.Any()) {
                _logger.LogWarning("No Aktors with quotes found for {Date}. Selecting random Aktor for Quote mode fallback.", date);
                quotePolitician = SelectWeightedRandom(allPoliticians.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Citat, date))).ToList());
            } else {
                quotePolitician = SelectWeightedRandom(quoteCandidates.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Citat, date))).ToList());
                if (quotePolitician?.Quotes?.Any() ?? false) 
                {
                     selectedQuote = quotePolitician.Quotes.ElementAt(_random.Next(quotePolitician.Quotes.Count)); 
                }
            }
            if (quotePolitician == null) { _logger.LogError("Could not select quote Aktor for {Date}.", date); return; }

            Aktor? photoPolitician;
            // Now using Aktor.Portraet (byte[])
            var photoCandidates = allPoliticians.Where(p => p.Portraet != null && p.Portraet.Length > 0).ToList();
            if (!photoCandidates.Any()) {
                _logger.LogWarning("No Aktors with portrait data found for {Date}. Selecting random Aktor for Photo mode fallback.", date);
                photoPolitician = SelectWeightedRandom(allPoliticians.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Foto, date))).ToList());
            } else {
                photoPolitician = SelectWeightedRandom(photoCandidates.Select(p => (politician: p, weight: CalculateSelectionWeight(p, GamemodeTypes.Foto, date))).ToList());
            }
            if (photoPolitician == null) { _logger.LogError("Could not select photo Aktor for {Date}.", date); return; }

            var dailySelections = new List<DailySelection> {
                new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Klassisk, SelectedPolitikerID = classicPolitician.Id },
                new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Citat, SelectedPolitikerID = quotePolitician.Id, SelectedQuoteText = selectedQuote?.QuoteText },
                new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Foto, SelectedPolitikerID = photoPolitician.Id }
            };
            _context.DailySelections.AddRange(dailySelections);

            await UpdateTrackerAsync(classicPolitician, GamemodeTypes.Klassisk, date);
            await UpdateTrackerAsync(quotePolitician, GamemodeTypes.Citat, date);
            await UpdateTrackerAsync(photoPolitician, GamemodeTypes.Foto, date);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully selected and saved daily Aktors for {Date}. Classic: {ClassicId}, Quote: {QuoteId}, Photo: {PhotoId}", date, classicPolitician.Id, quotePolitician.Id, photoPolitician.Id);
        }
        #endregion // Interface Implementation

        #region Helper Methods
        private int CalculateAge(DateOnly dateOfBirth, DateOnly referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate < dateOfBirth.AddYears(age)) age--; // More accurate check
            return Math.Max(0, age);
        }

        private async Task<int> GetSelectedPoliticianIdAsync(GamemodeTypes gameMode, DateOnly today)
        {
            var selection = await _context.DailySelections.AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);
            if (selection == null) 
            {
                _logger.LogError("No daily selection found for {GameMode} on {Date}. Attempting to select now.", gameMode, today);
                // Attempt to select one if missing (important for first run or if job failed)
                var politiker = await GetOrSelectDailyPoliticianAsync(gameMode);
                if (politiker == null)
                {
                    throw new KeyNotFoundException($"Could not select or find a daily politician for {gameMode} on {today} after fallback attempt.");
                }
                return politiker.Id;
            }
            return selection.SelectedPolitikerID;
        }
        #endregion Helper Methods

        #region Weighted Selection Helpers
        private async Task<Aktor?> PerformWeightedSelectionAsync(GamemodeTypes gameMode, DateOnly today)
        {
            const int maxWeightDays = 365 * 2; 
            const int defaultWeightIfNeverSelected = maxWeightDays + 1;
            _logger.LogDebug("Performing weighted selection for Aktor in {GameMode} on {Date}", gameMode, today);

            var candidatesQuery = _context.Aktor.Where(a => a.typeid == 5); 
                                          
            if (gameMode == GamemodeTypes.Citat)
            {
                candidatesQuery = candidatesQuery.Include(a => a.Quotes);
            }
            // No need to include Aktor.Portraet (byte[]) here as it's part of the main entity.
            // Aktor.Party (string) is also part of the main entity.
                                          
            var allAktorsForMode = await candidatesQuery
                .Include(a => a.GameTrackings.Where(gt => gt.GameMode == gameMode))
                .ToListAsync();

            if (!allAktorsForMode.Any()) { _logger.LogWarning("No Aktor candidates (typeid=5) found for weighted selection in {GameMode}.", gameMode); return null; }

            List<(Aktor politician, int weight)> weightedCandidates;

            if (gameMode == GamemodeTypes.Citat)
            {
                weightedCandidates = allAktorsForMode
                    .Where(a => a.Quotes != null && a.Quotes.Any()) // Only those with quotes
                    .Select(aktor => (
                        politician: aktor, 
                        weight: CalculateSelectionWeight(aktor, gameMode, today)
                    )).ToList();
                if (!weightedCandidates.Any()) // Fallback if no one has quotes
                {
                    _logger.LogWarning("No Aktors with quotes for Citat mode. Falling back to all Aktors for {GameMode}.", gameMode);
                    weightedCandidates = allAktorsForMode.Select(aktor => (politician: aktor, weight: CalculateSelectionWeight(aktor, gameMode, today))).ToList();
                }
            }
            else if (gameMode == GamemodeTypes.Foto)
            {
                weightedCandidates = allAktorsForMode
                    .Where(a => a.Portraet != null && a.Portraet.Length > 0) // Only those with portrait data
                    .Select(aktor => (
                        politician: aktor, 
                        weight: CalculateSelectionWeight(aktor, gameMode, today)
                    )).ToList();
                 if (!weightedCandidates.Any()) // Fallback if no one has portraits
                {
                    _logger.LogWarning("No Aktors with portrait data for Foto mode. Falling back to all Aktors for {GameMode}.", gameMode);
                    weightedCandidates = allAktorsForMode.Select(aktor => (politician: aktor, weight: CalculateSelectionWeight(aktor, gameMode, today))).ToList();
                }
            }
            else // Klassisk or other modes
            {
                 weightedCandidates = allAktorsForMode.Select(aktor => (
                    politician: aktor, 
                    weight: CalculateSelectionWeight(aktor, gameMode, today)
                )).ToList();
            }
            
            if (!weightedCandidates.Any()) { 
                _logger.LogWarning("No suitable weighted candidates found for {GameMode} after filtering.", gameMode); 
                return null; // Or handle fallback if allAktorsForMode had items but filtering removed all
            }

            return SelectWeightedRandom(weightedCandidates);
        }

        private int CalculateSelectionWeight(Aktor politician, GamemodeTypes gameMode, DateOnly currentDate)
        {
            const int maxWeightDays = 365 * 2; 
            const int defaultWeightIfNeverSelected = maxWeightDays + 1; 
            const int baseWeight = 1; 

            var tracker = politician.GameTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);

            if (tracker == null || !tracker.LastSelectedDate.HasValue) // Check HasValue for nullable DateOnly
            {
                return defaultWeightIfNeverSelected;
            }
            else
            {
                int daysSinceLastSelected = currentDate.DayNumber - tracker.LastSelectedDate.Value.DayNumber;
                return Math.Max(baseWeight, Math.Min(daysSinceLastSelected + baseWeight, maxWeightDays));
            }
        }

        private Aktor? SelectWeightedRandom(List<(Aktor politician, int weight)> weightedCandidates)
        {
            if (weightedCandidates == null || !weightedCandidates.Any()) 
            {
                _logger.LogWarning("SelectWeightedRandom called with no candidates.");
                return null;
            }

            var validCandidates = weightedCandidates.Where(c => c.weight > 0).ToList();
            if (!validCandidates.Any()) 
            {
                _logger.LogWarning("All candidates had non-positive weights. Performing simple random selection from original list (if any).");
                return weightedCandidates.Any() ? weightedCandidates[_random.Next(weightedCandidates.Count)].politician : null;
            }

            long totalWeight = validCandidates.Sum(c => (long)c.weight); 
            if (totalWeight <= 0) { // Should ideally not be reached if weights are > 0
                 _logger.LogError("Total weight is zero or less even with positive-weighted candidates. Fallback.");
                return validCandidates.Any() ? validCandidates[_random.Next(validCandidates.Count)].politician : null;
            }

            double randomValue = _random.NextDouble() * totalWeight;
            long cumulativeWeight = 0;
            foreach (var candidate in validCandidates)
            {
                cumulativeWeight += candidate.weight;
                if (randomValue < cumulativeWeight)
                {
                    return candidate.politician;
                }
            }
            
            _logger.LogError("Weighted random selection logic failed unexpectedly. Returning last valid candidate as fallback.");
            return validCandidates.LastOrDefault().politician; 
        }

        private async Task UpdateTrackerAsync(Aktor politician, GamemodeTypes gameMode, DateOnly selectionDate)
        {
            var existingTracker = politician.GameTrackings?.FirstOrDefault(gt => gt.GameMode == gameMode);
            
            if (existingTracker == null && politician.Id > 0) // Check politician.Id to ensure it's a valid entity
            {
                existingTracker = await _context.GameTrackings
                                            .FirstOrDefaultAsync(gt => gt.PolitikerId == politician.Id && gt.GameMode == gameMode);
            }

            if (existingTracker != null)
            {
                existingTracker.LastSelectedDate = selectionDate;
                existingTracker.AlgoWeight = null; 
                _context.GameTrackings.Update(existingTracker);
            }
            else if (politician.Id > 0) // Only add if politician is a valid entity (has an ID)
            {
                var newTracker = new PolidleGamemodeTracker
                {
                    PolitikerId = politician.Id, 
                    // Aktor = politician, // EF Core should link this via PolitikerId FK
                    GameMode = gameMode,
                    LastSelectedDate = selectionDate
                };
                await _context.GameTrackings.AddAsync(newTracker);
                
                politician.GameTrackings ??= new List<PolidleGamemodeTracker>();
                if(!politician.GameTrackings.Contains(newTracker))
                {
                    politician.GameTrackings.Add(newTracker);
                }
            }
            else
            {
                _logger.LogWarning("Skipped UpdateTrackerAsync for Aktor without ID. Politician: {PoliticianName}", politician.navn);
            }
        }
        #endregion
    }
}