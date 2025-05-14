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

        public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date)
        {
            _logger.LogInformation("Starting daily selection process for {Date}", date);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ... (din kode for at tjekke ExistsForDateAsync og hente allPoliticiansData) ...
                if (await _dailySelectionRepository.ExistsForDateAsync(date)) {
                    _logger.LogWarning("Daily selections already exist for {Date}. Skipping generation.", date);
                    await transaction.RollbackAsync();
                    return;
                }

                var allPoliticiansData = await _aktorRepository.GetAllWithDetailsForSelectionAsync();
                if (!allPoliticiansData.Any()) {
                    _logger.LogError("No politicians found. Cannot generate selections for {Date}.", date);
                    await transaction.RollbackAsync();
                    return;
                }
                _logger.LogInformation("Total Aktors fetched from repository: {Count}", allPoliticiansData.Count);
                // ... (din logning for politiciansWithQuotesCount) ...

                var candidatesClassic = allPoliticiansData
                    .Select(p => new CandidateData(p, p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Klassisk)))
                    .ToList();
                // ... (opret candidatesQuote og candidatesPhoto) ...

                // --- Vælg Classic Politician ---
                var classicPolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(candidatesClassic, date, GamemodeTypes.Klassisk);
                if (classicPolitician == null)
                {
                    _logger.LogError("CRITICAL: Could not select any classic politician for {Date}. Aborting selection process.", date);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Unable to select a classic politician for {date}.");
                }
                _logger.LogInformation("Classic Politician selected for {Date}: ID {AktorId} ({AktorNavn})", date, classicPolitician.Id, classicPolitician.navn);

                // --- Vælg Citat Politician og Citat ---
                Aktor? quotePolitician = null; // Start som null
                PoliticianQuote? selectedQuote = null;

                var candidatesQuote = allPoliticiansData // Sørg for at denne er defineret korrekt
                    .Where(p => p.Quotes != null && p.Quotes.Any(q => !string.IsNullOrWhiteSpace(q.QuoteText)))
                    .Select(p => new CandidateData(p, p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Citat)))
                    .ToList();

                if (candidatesQuote.Any())
                {
                    quotePolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(candidatesQuote, date, GamemodeTypes.Citat);
                }

                if (quotePolitician != null)
                {
                    // ... (din logik for at vælge selectedQuote fra quotePolitician.Quotes) ...
                    var validQuotesOnSelected = quotePolitician.Quotes?.Where(q => !string.IsNullOrWhiteSpace(q.QuoteText)).ToList();
                    if (validQuotesOnSelected?.Any() ?? false)
                    {
                        selectedQuote = validQuotesOnSelected[_randomProvider.Next(validQuotesOnSelected.Count)];
                    }
                }
                
                // Fallback for quotePolitician HVIS den stadig er null
                if (quotePolitician == null) {
                    _logger.LogWarning("Quote politician was not selected or had no quotes. Using classic politician as fallback for Quote Aktor on {Date}.", date);
                    quotePolitician = classicPolitician; // Fallback til classic
                    // Prøv igen at finde et citat på classicPolitician
                    var fallbackValidQuotes = quotePolitician.Quotes?.Where(q => !string.IsNullOrWhiteSpace(q.QuoteText)).ToList();
                    if (fallbackValidQuotes?.Any() ?? false)
                    {
                        selectedQuote = fallbackValidQuotes[_randomProvider.Next(fallbackValidQuotes.Count)];
                        _logger.LogInformation("Selected a quote from fallback Aktor {AktorId} for Citat mode: '{QuoteText}'", quotePolitician.Id, selectedQuote.QuoteText);
                    } else {
                        _logger.LogWarning("Fallback Aktor {AktorId} for Citat mode also has no valid quotes.", quotePolitician.Id);
                        selectedQuote = null; // Ingen citat fundet
                    }
                }


                // --- Vælg Foto Politician ---
                Aktor? photoPolitician = null; // Start som null

                var candidatesPhoto = allPoliticiansData // Sørg for at denne er defineret korrekt
                    .Where(p => !string.IsNullOrWhiteSpace(p.PictureMiRes))
                    .Select(p => new CandidateData(p, p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Foto)))
                    .ToList();

                if (candidatesPhoto.Any())
                {
                    photoPolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(candidatesPhoto, date, GamemodeTypes.Foto);
                }
                
                // Fallback for photoPolitician HVIS den stadig er null
                if (photoPolitician == null) {
                    _logger.LogWarning("Photo politician was not selected. Using classic politician as fallback for Photo Aktor on {Date}.", date);
                    photoPolitician = classicPolitician; // Fallback til classic
                }


                // --- Logning før oprettelse af DailySelection (LINJE 238 området) ---
                // Gør værdierne klar til logning, håndter nulls eksplicit
                var classicIdLog = classicPolitician.Id; // Vi ved classicPolitician ikke er null her
                var quoteAktorIdLog = quotePolitician.Id; // Vi har sikret quotePolitician ikke er null her (fallback til classic)
                var quoteTextLog = selectedQuote?.QuoteText ?? "INGEN_CITAT_VALGT";
                var photoAktorIdLog = photoPolitician.Id; // Vi har sikret photoPolitician ikke er null her (fallback til classic)

                _logger.LogInformation(
                    "Preparing DailySelection entities for {Date}. Classic: {ClassicId}, Quote Aktor: {QuoteAktorId} (Quote: '{QuoteText}'), Photo Aktor: {PhotoAktorId}",
                    date,
                    classicIdLog,
                    quoteAktorIdLog,
                    quoteTextLog, // Log den forberedte streng
                    photoAktorIdLog
                );

                var dailySelections = new List<DailySelection>
                {
                    new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Klassisk, SelectedPolitikerID = classicPolitician.Id },
                    new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Citat, SelectedPolitikerID = quotePolitician.Id, SelectedQuoteText = selectedQuote?.QuoteText },
                    new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Foto, SelectedPolitikerID = photoPolitician.Id }
                };
                await _dailySelectionRepository.AddManyAsync(dailySelections);
                // ... (Opdater Trackers) ...
                await _trackerRepository.UpdateOrCreateForAktorAsync(classicPolitician, GamemodeTypes.Klassisk, date);
                await _trackerRepository.UpdateOrCreateForAktorAsync(quotePolitician, GamemodeTypes.Citat, date); // quotePolitician er nu garanteret ikke null
                await _trackerRepository.UpdateOrCreateForAktorAsync(photoPolitician, GamemodeTypes.Foto, date); // photoPolitician er nu garanteret ikke null


                int changesSaved = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // --- Logning efter commit (LINJE 343 området) ---
                _logger.LogInformation(
                    "Final selections - Classic: {ClassicId}, Quote Aktor: {QuoteAktorId}, Photo Aktor: {PhotoAktorId}",
                    classicPolitician.Id, // classicPolitician er ikke null
                    quotePolitician.Id,   // quotePolitician er ikke null
                    photoPolitician.Id    // photoPolitician er ikke null
                );
                _logger.LogInformation("Successfully selected, saved daily politicians, and committed transaction for {Date}. Changes saved to DB: {ChangesCount}", date, changesSaved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during daily selection process for {Date}. Rolling back transaction.", date);
                await transaction.RollbackAsync();
                throw;
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