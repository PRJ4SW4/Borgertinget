// Fil: Services/DailySelectionService.cs
using backend.DTO;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;
using backend.Interfaces.Utility; // For IDateTimeProvider
using backend.Models;
using backend.Data;
using backend.Enums;
using backend.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Nødvendig for Transaction

namespace backend.Services
{
    public class DailySelectionService : IDailySelectionService
    {
        // --- Konstanter ---
        private static class FeedbackKeys
        {
            public const string Party = "Parti";
            public const string Gender = "Køn";
            public const string Region = "Region";
            public const string Education = "Uddannelse";
            public const string Age = "Alder";
        }

        // --- Dependencies ---
        private readonly IAktorRepository _aktorRepository;
        private readonly IDailySelectionRepository _dailySelectionRepository;
        private readonly IGamemodeTrackerRepository _trackerRepository;
        private readonly ISelectionAlgorithm _selectionAlgorithm;
        private readonly IPoliticianMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<DailySelectionService> _logger;
        private readonly IRandomProvider _randomProvider;
        private readonly DataContext _context; // Stadig nødvendig for Transaktion / Unit of Work


        public DailySelectionService(
            IAktorRepository aktorRepository,
            IDailySelectionRepository dailySelectionRepository,
            IGamemodeTrackerRepository trackerRepository,
            ISelectionAlgorithm selectionAlgorithm,
            IPoliticianMapper mapper,
            IDateTimeProvider dateTimeProvider,
            ILogger<DailySelectionService> logger,
            IRandomProvider randomProvider,
            DataContext context // Til transaktion,
            
            )
        {
            _aktorRepository = aktorRepository ?? throw new ArgumentNullException(nameof(aktorRepository));
            _dailySelectionRepository = dailySelectionRepository ?? throw new ArgumentNullException(nameof(dailySelectionRepository));
            _trackerRepository = trackerRepository ?? throw new ArgumentNullException(nameof(trackerRepository));
            _selectionAlgorithm = selectionAlgorithm ?? throw new ArgumentNullException(nameof(selectionAlgorithm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Til transaktion
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        // --- Public Methods ---

        public async Task<List<SearchListDto>> GetAllPoliticiansForGuessingAsync(string? search = null)
        {
            string sanitizedSearchForLog = LogSanitizer.Sanitize(search); // Rens til logning
            
            _logger.LogInformation("Fetching politicians for guessing. Search: '{SearchTerm}'", sanitizedSearchForLog); // <<< RETTET HER
            // Den *originale* 'search' streng bruges i _aktorRepository.GetAllForSummaryAsync(search)
            var aktors = await _aktorRepository.GetAllForSummaryAsync(search);
            var dtos = _mapper.MapToSummaryDtoList(aktors);
            _logger.LogInformation("Returning {Count} politician summaries. Search: '{SearchTerm}'", dtos.Count, sanitizedSearchForLog); // <<< RETTET HER
            return dtos;
        }

        public async Task<QuoteDto> GetQuoteOfTheDayAsync()
        {
            _logger.LogDebug("Getting quote of the day.");
            DateOnly today = _dateTimeProvider.TodayUtc;
            var selection = await _dailySelectionRepository.GetByDateAndModeAsync(today, GamemodeTypes.Citat);

            if (selection == null) throw new KeyNotFoundException($"Ingen DailySelection fundet for Citat d. {today}.");
            if (string.IsNullOrEmpty(selection.SelectedQuoteText)) throw new InvalidOperationException($"Citat-tekst mangler i DailySelection for {today}.");

            return new QuoteDto { QuoteText = selection.SelectedQuoteText };
        }

        public async Task<PhotoDto> GetPhotoOfTheDayAsync()
        {
             _logger.LogDebug("Getting photo of the day.");
             DateOnly today = _dateTimeProvider.TodayUtc;

             // Hent selection og Aktor sammen for at få URL
             var selection = await _dailySelectionRepository.GetByDateAndModeAsync(today, GamemodeTypes.Foto, includeAktor: true);

             if (selection?.SelectedPolitiker == null) {
                  bool exists = selection != null; // Fandtes selectionen, men ikke politikeren?
                  if (!exists) throw new KeyNotFoundException($"Ingen DailySelection fundet for Foto d. {today}.");
                  else throw new KeyNotFoundException($"Tilhørende Aktor for Foto d. {today} (ID: {selection?.SelectedPolitikerID}) blev ikke fundet/kunne ikke loades.");
             }

             if (string.IsNullOrWhiteSpace(selection.SelectedPolitiker.PictureMiRes)) {
                 throw new InvalidOperationException($"Billede URL (PictureMiRes) mangler for den valgte politiker (ID: {selection.SelectedPolitikerID}) til Foto d. {today}.");
             }

             return new PhotoDto { PhotoUrl = selection.SelectedPolitiker.PictureMiRes };
        }

        public async Task<DailyPoliticianDto> GetClassicDetailsOfTheDayAsync()
        {
             _logger.LogDebug("Getting classic details of the day.");
             DateOnly today = _dateTimeProvider.TodayUtc;

             var selection = await _dailySelectionRepository.GetByDateAndModeAsync(today, GamemodeTypes.Klassisk, includeAktor: true);

             if (selection?.SelectedPolitiker == null) {
                 bool exists = selection != null;
                 if (!exists) throw new KeyNotFoundException($"Ingen DailySelection fundet for Classic d. {today}.");
                 else throw new KeyNotFoundException($"Tilhørende Aktor for Classic d. {today} (ID: {selection?.SelectedPolitikerID}) blev ikke fundet/kunne ikke loades.");
             }

             return _mapper.MapToDetailsDto(selection.SelectedPolitiker); // Mapper klarer alder etc.
        }

        public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
        {
             _logger.LogInformation("Processing guess for GameMode {GameMode}, GuessedId {GuessedId}", guessDto.GameMode, guessDto.GuessedPoliticianId);
             DateOnly today = _dateTimeProvider.TodayUtc;

             // 1. Hent korrekt DailySelection og tilhørende Aktor
             var correctSelection = await _dailySelectionRepository.GetByDateAndModeAsync(today, guessDto.GameMode, includeAktor: true);
             if (correctSelection?.SelectedPolitiker == null) { /* ... exception som i GetClassicDetails ... */ throw new KeyNotFoundException($"Dagens valg for {guessDto.GameMode} d. {today} er ikke tilgængeligt."); }
             var correctPolitician = correctSelection.SelectedPolitiker;

             // 2. Hent gættet politiker (Aktor)
             var guessedPolitician = await _aktorRepository.GetByIdAsync(guessDto.GuessedPoliticianId, includeParty: true); // Antager vi skal bruge parti info
             if (guessedPolitician == null) throw new KeyNotFoundException($"Den gættede politiker med ID {guessDto.GuessedPoliticianId} blev ikke fundet.");

             // 3. Map begge til DTOs for nem sammenligning og resultat
             var correctPoliticianDto = _mapper.MapToDetailsDto(correctPolitician);
             var guessedPoliticianDto = _mapper.MapToDetailsDto(guessedPolitician);


             // 4. Byg resultat DTO'en
             var result = new GuessResultDto {
                 IsCorrectGuess = correctPolitician.Id == guessedPolitician.Id,
                 Feedback = new Dictionary<string, FeedbackType>(),
                 GuessedPolitician = guessedPoliticianDto
             };

             // 5. Udfør sammenligninger for Classic mode
             if (guessDto.GameMode == GamemodeTypes.Klassisk) {
                 CalculateClassicFeedback(result, correctPoliticianDto, guessedPoliticianDto);
             }
             // Andre modes har pt. kun IsCorrectGuess

             _logger.LogInformation("Guess result calculated for GuessedId {GuessedId}: IsCorrect={IsCorrect}", guessDto.GuessedPoliticianId, result.IsCorrectGuess);
             return result;
        }

        //TODO: Change to select new each call
        public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date, bool overwriteExisting = false)
        {
            _logger.LogInformation("Starting daily selection process for {Date}. Overwrite existing: {Overwrite}", date, overwriteExisting);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await _dailySelectionRepository.ExistsForDateAsync(date))
                {
                    if (overwriteExisting)
                    {
                        _logger.LogWarning("Daily selections already exist for {Date} but overwrite is requested. Deleting existing selections for this date.", date);
                        await _dailySelectionRepository.DeleteByDateAsync(date); // Antager denne metode nu findes og kalder SaveChanges internt eller markere for SaveChanges
                        // Hvis DeleteByDateAsync ikke kalder SaveChanges, og du vil have det gjort med det samme:
                        // await _context.SaveChangesAsync();
                        _logger.LogInformation("Existing daily selections for {Date} deleted due to overwrite request.", date);
                    }
                    else
                    {
                        _logger.LogWarning("Daily selections already exist for {Date} and overwrite is false. Skipping generation.", date);
                        await transaction.RollbackAsync();
                        return;
                    }
                }

                var allPoliticiansData = await _aktorRepository.GetAllWithDetailsForSelectionAsync();
                if (!allPoliticiansData.Any())
                {
                    _logger.LogError("No politicians found in the database. Cannot generate daily selections for {Date}.", date);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"No politicians available in database to generate selections for {date}.");
                }
                _logger.LogInformation("Total Aktors fetched from repository: {Count}", allPoliticiansData.Count);
                var politiciansWithQuotesCount = allPoliticiansData.Count(p => p.Quotes != null && p.Quotes.Any(q => !string.IsNullOrWhiteSpace(q.QuoteText)));
                _logger.LogInformation("Number of these Aktors with actual non-empty quotes: {WithQuotesCount}", politiciansWithQuotesCount);

                // --- Forbered kandidatlister ---
                var candidatesClassic = allPoliticiansData
                    .Select(p => new CandidateData(p, p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Klassisk)))
                    .ToList();

                var candidatesQuoteInternal = allPoliticiansData
                    .Where(p => p.Quotes != null && p.Quotes.Any(q => !string.IsNullOrWhiteSpace(q.QuoteText)))
                    .Select(p => new CandidateData(p, p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Citat)))
                    .ToList();
                _logger.LogInformation("Number of candidates for Quote Mode after filtering: {Count}", candidatesQuoteInternal.Count);

                var candidatesPhotoInternal = allPoliticiansData
                    .Where(p => !string.IsNullOrWhiteSpace(p.PictureMiRes))
                    .Select(p => new CandidateData(p, p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Foto)))
                    .ToList();
                _logger.LogInformation("Number of candidates for Foto Mode after filtering: {Count}", candidatesPhotoInternal.Count);

                // --- Vælg Classic Politician (skal altid vælges først, da den bruges som fallback) ---
                var classicPolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(candidatesClassic, date, GamemodeTypes.Klassisk);
                if (classicPolitician == null)
                {
                    _logger.LogError("CRITICAL: Could not select any classic politician for {Date}. Aborting selection process.", date);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Unable to select a classic politician for {date}. Check candidate list and algorithm.");
                }
                _logger.LogInformation("Classic Politician selected for {Date}: ID {AktorId} ({AktorNavn})", date, classicPolitician.Id, classicPolitician.navn);

                // --- Vælg Citat Politician og Citat ---
                Aktor? quotePolitician = null;
                PoliticianQuote? selectedQuote = null;

                if (candidatesQuoteInternal.Any())
                {
                    quotePolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(candidatesQuoteInternal, date, GamemodeTypes.Citat);
                }

                if (quotePolitician != null)
                {
                    _logger.LogInformation("Algorithm initially selected Aktor {AktorId} ({AktorNavn}) for Citat mode on {Date}.", quotePolitician.Id, quotePolitician.navn);
                    var validQuotesOnSelected = quotePolitician.Quotes?.Where(q => !string.IsNullOrWhiteSpace(q.QuoteText)).ToList();
                    if (validQuotesOnSelected?.Any() ?? false)
                    {
                        selectedQuote = validQuotesOnSelected[_randomProvider.Next(validQuotesOnSelected.Count)];
                        _logger.LogInformation("Successfully selected quote for Aktor {AktorId}: '{QuoteText}'", quotePolitician.Id, selectedQuote.QuoteText);
                    }
                    else
                    {
                        _logger.LogWarning("Aktor {AktorId} ({AktorNavn}), selected by algorithm for Citat, has no valid quotes. Applying fallback logic.", quotePolitician.Id, quotePolitician.navn);
                        // quotePolitician er allerede sat, men selectedQuote er stadig null. Fallback nedenfor vil forsøge at finde citat på denne eller classic.
                    }
                }
                else
                {
                    _logger.LogWarning("No specific Citat politician selected by algorithm (either no candidates or algorithm returned null). Applying fallback logic for {Date}.", date);
                }

                // Fallback logik for Citat mode, hvis enten quotePolitician eller selectedQuote er null
                if (quotePolitician == null || selectedQuote == null)
                {
                    _logger.LogInformation("Applying fallback for Citat mode Aktor and/or Quote for {Date}.", date);
                    quotePolitician = quotePolitician ?? classicPolitician; // Hvis quotePolitician er null, brug classic. Ellers behold den valgte.
                    _logger.LogInformation("Aktor for Citat mode is now ID {AktorId} ({AktorNavn}). Attempting to find a quote.", quotePolitician.Id, quotePolitician.navn);

                    var fallbackValidQuotes = quotePolitician.Quotes?.Where(q => !string.IsNullOrWhiteSpace(q.QuoteText)).ToList();
                    if (fallbackValidQuotes?.Any() ?? false)
                    {
                        selectedQuote = fallbackValidQuotes[_randomProvider.Next(fallbackValidQuotes.Count)];
                        _logger.LogInformation("Selected a quote from Aktor {AktorId} for Citat mode: '{QuoteText}'", quotePolitician.Id, selectedQuote.QuoteText);
                    }
                    else
                    {
                        _logger.LogWarning("Aktor {AktorId} ({AktorNavn}) for Citat mode has no valid quotes, even after fallback. SelectedQuoteText will be null.", quotePolitician.Id, quotePolitician.navn);
                        selectedQuote = null;
                    }
                }

                if (quotePolitician == null) quotePolitician = classicPolitician;

                // --- Vælg Foto Politician ---
                Aktor? photoPolitician = null;
                if (candidatesPhotoInternal.Any())
                {
                    photoPolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(candidatesPhotoInternal, date, GamemodeTypes.Foto);
                }

                if (photoPolitician == null) // Fallback for Foto mode
                {
                    _logger.LogWarning("No specific Foto politician selected by algorithm or no candidates. Using classic politician as fallback for Photo Aktor on {Date}.", date);
                    photoPolitician = classicPolitician;
                }
                _logger.LogInformation("Photo Politician selected for {Date}: ID {AktorId} ({AktorNavn})", date, photoPolitician.Id, photoPolitician.navn);


                // --- Logning før oprettelse af DailySelection (sikrer at objekter ikke er null) ---
                _logger.LogInformation(
                    "Preparing DailySelection entities for {Date}. Classic: {ClassicId} ({ClassicName}), Quote Aktor: {QuoteAktorId} ({QuoteAktorName}), Quote: '{QuoteText}', Photo Aktor: {PhotoAktorId} ({PhotoAktorName})",
                    date,
                    classicPolitician.Id, // classicPolitician er garanteret non-null
                    classicPolitician.navn ?? "N/A",
                    quotePolitician.Id,   // quotePolitician er nu garanteret non-null (fallback til classic)
                    quotePolitician.navn ?? "N/A",
                    selectedQuote?.QuoteText ?? "INGEN_CITAT_VALGT",
                    photoPolitician.Id,   // photoPolitician er nu garanteret non-null (fallback til classic)
                    photoPolitician.navn ?? "N/A"
                );

                var dailySelections = new List<DailySelection>
                {
                    new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Klassisk, SelectedPolitikerID = classicPolitician.Id },
                    new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Citat, SelectedPolitikerID = quotePolitician.Id, SelectedQuoteText = selectedQuote?.QuoteText },
                    new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Foto, SelectedPolitikerID = photoPolitician.Id }
                };
                await _dailySelectionRepository.AddManyAsync(dailySelections);
                _logger.LogInformation("DailySelection entities prepared and added to context for {Date}.", date);

                // --- Opdater Trackers ---
                await _trackerRepository.UpdateOrCreateForAktorAsync(classicPolitician, GamemodeTypes.Klassisk, date);
                await _trackerRepository.UpdateOrCreateForAktorAsync(quotePolitician, GamemodeTypes.Citat, date);
                await _trackerRepository.UpdateOrCreateForAktorAsync(photoPolitician, GamemodeTypes.Foto, date);
                _logger.LogInformation("GamemodeTrackers updated/created for {Date}.", date);

                // Gem alle ændringer
                int changesSaved = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // --- Logning efter commit (sikrer at objekter ikke er null) ---
                _logger.LogInformation(
                    "Final selections for {Date} - Classic: {ClassicId} ({ClassicName}), Quote Aktor: {QuoteAktorId} ({QuoteAktorName}), Photo Aktor: {PhotoAktorId} ({PhotoAktorName}). Changes: {ChangesCount}",
                    date, // Tilføjet {Date} placeholder
                    classicPolitician.Id,
                    classicPolitician.navn ?? "N/A",
                    quotePolitician.Id,
                    quotePolitician.navn ?? "N/A",
                    photoPolitician.Id,
                    photoPolitician.navn ?? "N/A",
                    changesSaved // Tilføjet {ChangesCount} placeholder
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during daily selection process for {Date} (Overwrite: {Overwrite}). Rolling back transaction.", date, overwriteExisting);
                await transaction.RollbackAsync();
                throw;
            }
        }

        //*FIXED: Implement method for seeding Quotes to Aktors
        private static readonly List<string> GenericQuotesForSeeding = new List<string>
        {
            "Fremtiden kræver modige beslutninger og fælles ansvar.",
            "Vi skal sikre et Danmark i balance, både socialt og økonomisk.",
            // ... (indsæt resten af dine 20+ generiske citater her) ...
            "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder.",
            "Investering i uddannelse og forskning er investering i vores fremtid.",
            "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed.",
            "Dialog og samarbejde på tværs af partiskel er vejen frem.",
            "Det lokale engagement er drivkraften i et levende demokrati.",
            "Vi skal turde tænke nyt for at løse fremtidens udfordringer.",
            "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand.",
            "Alle borgere fortjener respekt og en fair behandling af systemet.",
            "Transparens og åbenhed er afgørende for tilliden til det politiske system.",
            "Vi skal værne om de danske værdier og vores kulturelle arv.",
            "Internationalt samarbejde er essentielt i en globaliseret verden.",
            "En robust økonomi giver os råderum til at investere i velfærd.",
            "Børns trivsel og udvikling skal altid have førsteprioritet.",
            "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund.",
            "Digitalisering byder på store muligheder, men kræver også omtanke.",
            "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark.",
            "Retssikkerhed og lighed for loven er grundpiller i vores demokrati.",
            "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer.",
            "Innovation og iværksætteri er nøglen til fremtidig vækst.",
            "En effektiv offentlig sektor er en service for borgerne.",
            "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse.",
            "Vi har et ansvar for at efterlade en bedre verden til de næste generationer.",
            "Forebyggelse er ofte bedre og billigere end reparation."
        };

        public async Task<string> SeedQuotesForAllAktorsAsync()
        {
            _logger.LogInformation("Starting to seed quotes for all Aktors.");
            if (!GenericQuotesForSeeding.Any() || GenericQuotesForSeeding.Count < 2)
            {
                const string errorMsg = "Not enough generic quotes available for seeding (need at least 2).";
                _logger.LogError(errorMsg);
                return errorMsg;
            }

            var allAktors = await _context.Aktor // Brug _context direkte her, eller _aktorRepository
                .Include(a => a.Quotes) // Vigtigt at inkludere eksisterende citater
                .Where(a => a.typeid == 5) // Antager du kun vil have citater for typeid 5
                .ToListAsync();

            if (!allAktors.Any())
            {
                const string msg = "No Aktors (typeid 5) found in the database to seed quotes for.";
                _logger.LogWarning(msg);
                return msg;
            }

            int aktorsProcessed = 0;
            int quotesAddedTotal = 0;
            int genericQuotePoolSize = GenericQuotesForSeeding.Count;
            int currentGenericQuoteIndex = _randomProvider.Next(genericQuotePoolSize);

            List<PoliticianQuote> quotesToAdd = new List<PoliticianQuote>();

            foreach (var aktor in allAktors)
            {
                // Tæl kun gyldige, eksisterende citater
                int existingValidQuotesCount = aktor.Quotes?.Count(q => !string.IsNullOrWhiteSpace(q.QuoteText)) ?? 0;
                int quotesNeeded = 2 - existingValidQuotesCount;

                if (quotesNeeded > 0)
                {
                    _logger.LogDebug("Aktor {AktorId} ('{AktorNavn}') needs {QuotesNeeded} quotes. Currently has {CurrentCount} valid quotes.",
                        aktor.Id, aktor.navn, quotesNeeded, existingValidQuotesCount);

                    for (int i = 0; i < quotesNeeded; i++)
                    {
                        string quoteText = GenericQuotesForSeeding[currentGenericQuoteIndex % genericQuotePoolSize];
                        currentGenericQuoteIndex++;

                        var newQuote = new PoliticianQuote
                        {
                            // INGEN QuoteId her - databasen skal generere den
                            AktorId = aktor.Id,
                            QuoteText = quoteText
                            // Politician navigation property sættes automatisk af EF Core pga. AktorId
                        };
                        quotesToAdd.Add(newQuote); // Tilføj til en liste først
                        quotesAddedTotal++;
                    }
                    aktorsProcessed++;
                }
            }

            if (quotesToAdd.Any())
            {
                // Tilføj alle nye citater til context i én omgang
                _context.PoliticianQuotes.AddRange(quotesToAdd); // <<< BRUG AddRange
                await _context.SaveChangesAsync(); // Gem alle nye citater
                string successMsg = $"Successfully added {quotesAddedTotal} quotes for {aktorsProcessed} Aktors.";
                _logger.LogInformation(successMsg);
                return successMsg;
            }
            else
            {
                const string noActionMsg = "No Aktors needed new quotes, or no Aktors (typeid 5) found.";
                _logger.LogInformation(noActionMsg);
                return noActionMsg;
            }
        }
        // --- Private Helper Methods ---

        private void CalculateClassicFeedback(GuessResultDto result, DailyPoliticianDto correctDto, DailyPoliticianDto guessedDto)
        {
             // Antager at IsCorrectGuess allerede er sat, og at result.Feedback er initialiseret
             if (result.IsCorrectGuess) return; // Ingen grund til feedback hvis gættet er korrekt

             result.Feedback[FeedbackKeys.Party] = string.Equals(correctDto.Parti, guessedDto.Parti) ? FeedbackType.Korrekt : FeedbackType.Forkert;
             result.Feedback[FeedbackKeys.Gender] = string.Equals(correctDto.Køn, guessedDto.Køn, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
             result.Feedback[FeedbackKeys.Region] = string.Equals(correctDto.Region, guessedDto.Region, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
             result.Feedback[FeedbackKeys.Education] = string.Equals(correctDto.Uddannelse, guessedDto.Uddannelse, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;

             if (correctDto.Age == guessedDto.Age) result.Feedback[FeedbackKeys.Age] = FeedbackType.Korrekt;
             else if (correctDto.Age > guessedDto.Age) result.Feedback[FeedbackKeys.Age] = FeedbackType.Højere;
             else result.Feedback[FeedbackKeys.Age] = FeedbackType.Lavere;
        }
    }
}