using backend.Data;
using backend.DTO;
using backend.Enums;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;
using backend.Interfaces.Utility;
using backend.Models;
using backend.Models.Politicians;
using backend.Utils;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DailySelectionService : IDailySelectionService
    {
        // --- Konstanter ---
        private static class FeedbackKeys
        {
            public const string PartyShortname = "PartyShortname";
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
        private readonly DataContext _context;

        public DailySelectionService(
            IAktorRepository aktorRepository,
            IDailySelectionRepository dailySelectionRepository,
            IGamemodeTrackerRepository trackerRepository,
            ISelectionAlgorithm selectionAlgorithm,
            IPoliticianMapper mapper,
            IDateTimeProvider dateTimeProvider,
            ILogger<DailySelectionService> logger,
            IRandomProvider randomProvider,
            DataContext context
        )
        {
            _aktorRepository =
                aktorRepository ?? throw new ArgumentNullException(nameof(aktorRepository));
            _dailySelectionRepository =
                dailySelectionRepository
                ?? throw new ArgumentNullException(nameof(dailySelectionRepository));
            _trackerRepository =
                trackerRepository ?? throw new ArgumentNullException(nameof(trackerRepository));
            _selectionAlgorithm =
                selectionAlgorithm ?? throw new ArgumentNullException(nameof(selectionAlgorithm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dateTimeProvider =
                dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _randomProvider =
                randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        // --- Public Methods ---
        public async Task<List<SearchListDto>> GetAllPoliticiansForGuessingAsync(
            string? search = null
        )
        {
            string sanitizedSearchForLog = LogSanitizer.Sanitize(search);

            _logger.LogInformation(
                "Fetching politicians for guessing. Search: '{SearchTerm}'",
                sanitizedSearchForLog
            );
            var aktors = await _aktorRepository.GetAllForSummaryAsync(search);
            var dtos = _mapper.MapToSummaryDtoList(aktors);
            _logger.LogInformation(
                "Returning {Count} politician summaries. Search: '{SearchTerm}'",
                dtos.Count,
                sanitizedSearchForLog
            );
            return dtos;
        }

        public async Task<QuoteDto?> GetQuoteOfTheDayAsync()
        {
            _logger.LogDebug("Getting quote of the day.");
            DateOnly today = _dateTimeProvider.TodayUtc;
            var selection = await _dailySelectionRepository.GetByDateAndModeAsync(
                today,
                GamemodeTypes.Citat
            );
            if (selection == null)
                throw new KeyNotFoundException(
                    $"Ingen DailySelection fundet for Citat d. {today}."
                );
            if (string.IsNullOrEmpty(selection.SelectedQuoteText))
                throw new InvalidOperationException(
                    $"Citat-tekst mangler i DailySelection for {today}."
                );

            return new QuoteDto { QuoteText = selection.SelectedQuoteText };
        }

        public async Task<PhotoDto?> GetPhotoOfTheDayAsync()
        {
            _logger.LogDebug("Getting photo of the day.");
            DateOnly today = _dateTimeProvider.TodayUtc;

            var selection = await _dailySelectionRepository.GetByDateAndModeAsync(
                today,
                GamemodeTypes.Foto,
                includeAktor: true
            );

            if (selection?.SelectedPolitiker == null)
            {
                bool exists = selection != null;
                if (!exists)
                    throw new KeyNotFoundException(
                        $"Ingen DailySelection fundet for Foto d. {today}."
                    );
                else
                    throw new KeyNotFoundException(
                        $"Tilhørende Aktor for Foto d. {today} (ID: {selection?.SelectedPolitikerID}) blev ikke fundet/kunne ikke loades."
                    );
            }

            if (string.IsNullOrWhiteSpace(selection.SelectedPolitiker.PictureMiRes))
            {
                throw new InvalidOperationException(
                    $"Billede URL (PictureMiRes) mangler for den valgte politiker (ID: {selection.SelectedPolitikerID}) til Foto d. {today}."
                );
            }

            return new PhotoDto { PhotoUrl = selection.SelectedPolitiker.PictureMiRes };
        }

        public async Task<DailyPoliticianDto?> GetClassicDetailsOfTheDayAsync()
        {
            _logger.LogDebug("Getting classic details of the day.");
            DateOnly today = _dateTimeProvider.TodayUtc;

            var selection = await _dailySelectionRepository.GetByDateAndModeAsync(
                today,
                GamemodeTypes.Klassisk,
                includeAktor: true
            );

            if (selection?.SelectedPolitiker == null)
            {
                bool exists = selection != null;
                if (!exists)
                    throw new KeyNotFoundException(
                        $"Ingen DailySelection fundet for Classic d. {today}."
                    );
                else
                    throw new KeyNotFoundException(
                        $"Tilhørende Aktor for Classic d. {today} (ID: {selection?.SelectedPolitikerID}) blev ikke fundet/kunne ikke loades."
                    );
            }

            return _mapper.MapToDetailsDto(selection.SelectedPolitiker);
        }

        public async Task<GuessResultDto?> ProcessGuessAsync(GuessRequestDto guessDto)
        {
            _logger.LogInformation(
                "Processing guess for GameMode {GameMode}, GuessedId {GuessedId}",
                guessDto.GameMode,
                guessDto.GuessedPoliticianId
            );
            DateOnly today = _dateTimeProvider.TodayUtc;

            // 1. Hent korrekt DailySelection og tilhørende Aktor
            var correctSelection = await _dailySelectionRepository.GetByDateAndModeAsync(
                today,
                guessDto.GameMode,
                includeAktor: true
            );
            if (correctSelection?.SelectedPolitiker == null)
            {
                throw new KeyNotFoundException(
                    $"Dagens valg for {guessDto.GameMode} d. {today} er ikke tilgængeligt."
                );
            }
            var correctPolitician = correctSelection.SelectedPolitiker;

            // 2. Hent gættet politiker (Aktor)
            var guessedPolitician = await _aktorRepository.GetByIdAsync(
                guessDto.GuessedPoliticianId,
                includeParty: true
            );
            if (guessedPolitician == null)
                throw new KeyNotFoundException(
                    $"Den gættede politiker med ID {guessDto.GuessedPoliticianId} blev ikke fundet."
                );

            // 3. Map begge til DTOs for nem sammenligning og resultat
            var correctPoliticianDto = _mapper.MapToDetailsDto(correctPolitician);
            var guessedPoliticianDto = _mapper.MapToDetailsDto(guessedPolitician);

            // 4. Byg resultat DTO'en
            var result = new GuessResultDto
            {
                IsCorrectGuess = correctPolitician.Id == guessedPolitician.Id,
                Feedback = new Dictionary<string, FeedbackType>(),
                GuessedPolitician = guessedPoliticianDto,
            };

            // 5. Udfør sammenligninger for Classic mode
            if (guessDto.GameMode == GamemodeTypes.Klassisk)
            {
                CalculateClassicFeedback(result, correctPoliticianDto, guessedPoliticianDto);
            }

            _logger.LogInformation(
                "Guess result calculated for GuessedId {GuessedId}: IsCorrect={IsCorrect}",
                guessDto.GuessedPoliticianId,
                result.IsCorrectGuess
            );
            return result;
        }

        public async Task SelectAndSaveDailyPoliticiansAsync(
            DateOnly date,
            bool overwriteExisting = false
        )
        {
            _logger.LogInformation(
                "METHOD_START: SelectAndSaveDailyPoliticiansAsync for {Date}. Overwrite: {Overwrite}",
                date,
                overwriteExisting
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await _dailySelectionRepository.ExistsForDateAsync(date))
                {
                    if (overwriteExisting)
                    {
                        _logger.LogWarning(
                            "Overwrite requested for {Date}. Deleting existing.",
                            date
                        );
                        await _dailySelectionRepository.DeleteByDateAsync(date);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Selections for {Date} exist and overwrite is false. Skipping.",
                            date
                        );
                        await transaction.RollbackAsync();
                        return;
                    }
                }

                var allPoliticiansData =
                    await _aktorRepository.GetAllWithDetailsForSelectionAsync();
                if (!allPoliticiansData.Any())
                {
                    _logger.LogError("No politicians in DB for {Date}.", date);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"No politicians in DB for {date}.");
                }
                var candidatesClassic = allPoliticiansData
                    .Select(p => new CandidateData(
                        p,
                        p.GamemodeTrackings?.FirstOrDefault(t =>
                            t.GameMode == GamemodeTypes.Klassisk
                        )
                    ))
                    .ToList();

                var classicPolitician = _selectionAlgorithm.SelectWeightedRandomCandidate(
                    candidatesClassic,
                    date,
                    GamemodeTypes.Klassisk
                );
                if (classicPolitician == null)
                {
                    _logger.LogError("CRITICAL_ERROR: classicPolitician is NULL for {Date}.", date);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException(
                        $"Cannot select classic politician for {date}."
                    );
                }

                // --- Citat Logik ---
                Aktor quoteAktorForSelection = classicPolitician;
                PoliticianQuote? selectedQuote = null;
                var candidatesQuoteInternal = allPoliticiansData
                    .Where(p =>
                        p.Quotes != null
                        && p.Quotes.Any(q => !string.IsNullOrWhiteSpace(q.QuoteText))
                    )
                    .Select(p => new CandidateData(
                        p,
                        p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Citat)
                    ))
                    .ToList();

                if (candidatesQuoteInternal.Any())
                {
                    var initiallySelectedQuoteAktor =
                        _selectionAlgorithm.SelectWeightedRandomCandidate(
                            candidatesQuoteInternal,
                            date,
                            GamemodeTypes.Citat
                        );
                    if (initiallySelectedQuoteAktor != null)
                    {
                        quoteAktorForSelection = initiallySelectedQuoteAktor;
                        var validQuotes = quoteAktorForSelection
                            .Quotes?.Where(q => !string.IsNullOrWhiteSpace(q.QuoteText))
                            .ToList();
                        if (validQuotes?.Any() ?? false)
                        {
                            selectedQuote = validQuotes[_randomProvider.Next(validQuotes.Count)];
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Aktor {AktorId} for Citat had no valid quotes, selectedQuote remains null.",
                                quoteAktorForSelection.Id
                            );
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Algorithm returned null for Citat candidates. Using classic for Citat Aktor."
                        );
                    }
                }
                else
                {
                    _logger.LogWarning("No candidates with quotes. Using classic for Citat Aktor.");
                }
                if (selectedQuote == null && quoteAktorForSelection.Id == classicPolitician.Id)
                {
                    var classicValidQuotes = classicPolitician
                        .Quotes?.Where(q => !string.IsNullOrWhiteSpace(q.QuoteText))
                        .ToList();
                    if (classicValidQuotes?.Any() ?? false)
                    {
                        selectedQuote = classicValidQuotes[
                            _randomProvider.Next(classicValidQuotes.Count)
                        ];
                        _logger.LogInformation(
                            "Selected quote from Classic Aktor for Citat mode as fallback."
                        );
                    }
                }

                // --- Foto Logik ---
                Aktor photoAktorForSelection = classicPolitician;
                var candidatesPhotoInternal = allPoliticiansData
                    .Where(p => !string.IsNullOrWhiteSpace(p.PictureMiRes))
                    .Select(p => new CandidateData(
                        p,
                        p.GamemodeTrackings?.FirstOrDefault(t => t.GameMode == GamemodeTypes.Foto)
                    ))
                    .ToList();
                if (candidatesPhotoInternal.Any())
                {
                    var initiallySelectedPhotoAktor =
                        _selectionAlgorithm.SelectWeightedRandomCandidate(
                            candidatesPhotoInternal,
                            date,
                            GamemodeTypes.Foto
                        );
                    if (initiallySelectedPhotoAktor != null)
                    {
                        photoAktorForSelection = initiallySelectedPhotoAktor;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Algorithm returned null for Foto candidates. Using classic for Foto Aktor."
                        );
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "No candidates with pictures. Using classic for Foto Aktor."
                    );
                }

                string logDate = date.ToString("yyyy-MM-dd");
                string logClassicId = classicPolitician.Id.ToString();
                string logClassicName = classicPolitician.navn ?? "IKKE_ANGIVET_NAVN_CLASSIC";
                string logQuoteAktorId = quoteAktorForSelection.Id.ToString();
                string logQuoteAktorName = quoteAktorForSelection.navn ?? "IKKE_ANGIVET_NAVN_QUOTE";
                string logQuoteText = selectedQuote?.QuoteText ?? "INGEN_CITAT_VALGT";
                string logPhotoAktorId = photoAktorForSelection.Id.ToString();
                string logPhotoAktorName = photoAktorForSelection.navn ?? "IKKE_ANGIVET_NAVN_FOTO";

                _logger.LogInformation(
                    "PREP_DS: Date:{Date}, ClsId:{ClassicId}({ClassicName}), QteAktorId:{QuoteAktorId}({QuoteAktorName}), QteTxt:'{QuoteText}', PhoAktorId:{PhotoAktorId}({PhotoAktorName})",
                    logDate,
                    logClassicId,
                    logClassicName,
                    logQuoteAktorId,
                    logQuoteAktorName,
                    logQuoteText,
                    logPhotoAktorId,
                    logPhotoAktorName
                );

                var dailySelections = new List<DailySelection>
                {
                    new DailySelection
                    {
                        SelectionDate = date,
                        GameMode = GamemodeTypes.Klassisk,
                        SelectedPolitikerID = classicPolitician.Id,
                    },
                    new DailySelection
                    {
                        SelectionDate = date,
                        GameMode = GamemodeTypes.Citat,
                        SelectedPolitikerID = quoteAktorForSelection.Id,
                        SelectedQuoteText = selectedQuote?.QuoteText,
                    },
                    new DailySelection
                    {
                        SelectionDate = date,
                        GameMode = GamemodeTypes.Foto,
                        SelectedPolitikerID = photoAktorForSelection.Id,
                    },
                };
                await _dailySelectionRepository.AddManyAsync(dailySelections);
                _logger.LogInformation(
                    "DS_ADDED_CTX: DailySelections for {Date} prepared for context.",
                    date
                );

                await _trackerRepository.UpdateOrCreateForAktorAsync(
                    classicPolitician,
                    GamemodeTypes.Klassisk,
                    date
                );
                await _trackerRepository.UpdateOrCreateForAktorAsync(
                    quoteAktorForSelection,
                    GamemodeTypes.Citat,
                    date
                );
                await _trackerRepository.UpdateOrCreateForAktorAsync(
                    photoAktorForSelection,
                    GamemodeTypes.Foto,
                    date
                );
                _logger.LogInformation("TRACKERS_UPDATED: Trackers for {Date} updated.", date);

                int changesSaved = await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation(
                    "CHANGES_SAVED: {ChangesCount} for {Date}",
                    changesSaved,
                    date
                );

                string finalChangesSavedStr = changesSaved.ToString();

                _logger.LogInformation(
                    "FINAL_SELECTIONS: Date:{Date} - ClsId:{ClassicId}({ClassicName}), QteAktorId:{QuoteAktorId}({QuoteAktorName}), PhoAktorId:{PhotoAktorId}({PhotoAktorName}). Changes:{ChangesCount}",
                    logDate,
                    logClassicId,
                    logClassicName,
                    logQuoteAktorId,
                    logQuoteAktorName,
                    logPhotoAktorId,
                    logPhotoAktorName,
                    finalChangesSavedStr
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR_PROCESS: Daily selection for {Date} (Overwrite:{Overwrite}) failed. Rolling back.",
                    date,
                    overwriteExisting
                );
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static readonly List<string> GenericQuotesForSeeding = new List<string>
        {
            "Fremtiden kræver modige beslutninger og fælles ansvar.",
            "Vi skal sikre et Danmark i balance, både socialt og økonomisk.",
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
            "Forebyggelse er ofte bedre og billigere end reparation.",
        };

        public async Task<string> SeedQuotesForAllAktorsAsync()
        {
            _logger.LogInformation("Starting to seed quotes for all Aktors.");
            if (!GenericQuotesForSeeding.Any() || GenericQuotesForSeeding.Count < 2)
            {
                const string errorMsg =
                    "Not enough generic quotes available for seeding (need at least 2).";
                _logger.LogError(errorMsg);
                return errorMsg;
            }

            var allAktors = await _context
                .Aktor
                .Include(a => a.Quotes)
                .Where(a => a.typeid == 5)
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
                int existingValidQuotesCount =
                    aktor.Quotes?.Count(q => !string.IsNullOrWhiteSpace(q.QuoteText)) ?? 0;
                int quotesNeeded = 2 - existingValidQuotesCount;

                if (quotesNeeded > 0)
                {
                    _logger.LogDebug(
                        "Aktor {AktorId} ('{AktorNavn}') needs {QuotesNeeded} quotes. Currently has {CurrentCount} valid quotes.",
                        aktor.Id,
                        aktor.navn,
                        quotesNeeded,
                        existingValidQuotesCount
                    );

                    for (int i = 0; i < quotesNeeded; i++)
                    {
                        string quoteText = GenericQuotesForSeeding[
                            currentGenericQuoteIndex % genericQuotePoolSize
                        ];
                        currentGenericQuoteIndex++;

                        var newQuote = new PoliticianQuote
                        {
                            AktorId = aktor.Id,
                            QuoteText = quoteText,
                        };
                        quotesToAdd.Add(newQuote);
                        quotesAddedTotal++;
                    }
                    aktorsProcessed++;
                }
            }

            if (quotesToAdd.Any())
            {
                _context.PoliticianQuotes.AddRange(quotesToAdd);
                await _context.SaveChangesAsync();
                string successMsg =
                    $"Successfully added {quotesAddedTotal} quotes for {aktorsProcessed} Aktors.";
                _logger.LogInformation(successMsg);
                return successMsg;
            }
            else
            {
                const string noActionMsg =
                    "No Aktors needed new quotes, or no Aktors (typeid 5) found.";
                _logger.LogInformation(noActionMsg);
                return noActionMsg;
            }
        }

        // --- Private Helper Methods ---

        private void CalculateClassicFeedback(
            GuessResultDto result,
            DailyPoliticianDto correctDto,
            DailyPoliticianDto guessedDto
        )
        {
            if (result.IsCorrectGuess)
                return;

            result.Feedback[FeedbackKeys.PartyShortname] = string.Equals(
                correctDto.PartyShortname,
                guessedDto.PartyShortname
            )
                ? FeedbackType.Korrekt
                : FeedbackType.Forkert;
            result.Feedback[FeedbackKeys.Gender] = string.Equals(
                correctDto.Køn,
                guessedDto.Køn,
                StringComparison.OrdinalIgnoreCase
            )
                ? FeedbackType.Korrekt
                : FeedbackType.Forkert;
            result.Feedback[FeedbackKeys.Region] = string.Equals(
                correctDto.Region,
                guessedDto.Region,
                StringComparison.OrdinalIgnoreCase
            )
                ? FeedbackType.Korrekt
                : FeedbackType.Forkert;
            result.Feedback[FeedbackKeys.Education] = string.Equals(
                correctDto.Uddannelse,
                guessedDto.Uddannelse,
                StringComparison.OrdinalIgnoreCase
            )
                ? FeedbackType.Korrekt
                : FeedbackType.Forkert;

            if (correctDto.Age == guessedDto.Age)
                result.Feedback[FeedbackKeys.Age] = FeedbackType.Korrekt;
            else if (correctDto.Age > guessedDto.Age)
                result.Feedback[FeedbackKeys.Age] = FeedbackType.Højere;
            else
                result.Feedback[FeedbackKeys.Age] = FeedbackType.Lavere;
        }
    }
}
