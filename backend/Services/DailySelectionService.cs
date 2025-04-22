using backend.Models; // Adgang til dine Entities (FakePolitiker, DailySelection, PoliticianQuote etc.)
using backend.DTO; // Adgang til DTOs (QuoteDto, PhotoDto, GuessResultDto etc.)
using backend.Data; // Adgang til din DataContext
using Microsoft.EntityFrameworkCore; // For Include, FirstOrDefaultAsync etc.
using Microsoft.Extensions.Logging; // Til logging
using System; // For DateOnly, Random, Math etc.
using System.Collections.Generic; // For List<>
using System.Linq; // For LINQ metoder som Where, Select, Any, OrderBy etc.
using System.Threading.Tasks; // For async/await

namespace backend.Services
{
    public class DailySelectionService : IDailySelectionService
    {
        #region Fields and Constructor

        private readonly DataContext _context; // Din DbContext klasse
        private readonly ILogger<DailySelectionService> _logger;
        private readonly Random _random = new Random(); // Til tilfældig udvælgelse

        // Constructor til Dependency Injection
        public DailySelectionService(DataContext context, ILogger<DailySelectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #endregion // Fields and Constructor

        #region Helper Methods

        // --- HJÆLPEMETODE til Aldersberegning ---
        private int CalculateAge(DateOnly dateOfBirth, DateOnly referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;
            // Gå et år ned hvis fødselsdagen ikke er passeret i referenceåret
            if (referenceDate.DayOfYear < dateOfBirth.DayOfYear)
            {
                age--;
            }
            // Sikrer at alder ikke er negativ
            return Math.Max(0, age);
        }

        // --- HJÆLPEMETODE: Hent ID for dagens politiker for en given gamemode ---
        private async Task<int> GetSelectedPoliticianIdAsync(GamemodeTypes gameMode, DateOnly today)
        {
             // Find den relevante række i DailySelections tabellen
             var selection = await _context.DailySelections
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);

            // Håndter hvis intet valg findes for dagen (bør ikke ske hvis jobbet kører)
            if (selection == null)
            {
                 _logger.LogError("No daily selection found for {GameMode} on {Date}. Run the daily selection job.", gameMode, today);
                 throw new KeyNotFoundException($"Ingen dagens politiker fundet for {gameMode} d. {today}.");
            }
            // Returner ID'et
            return selection.SelectedPolitikerID;
        }

        #endregion // Helper Methods

        #region API Methods - Data Retrieval

        // --- API METODE: Hent liste af politikere til gætte-input ---
        public async Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync()
        {
             // Hent alle politikere, sorter efter navn, og map til DTO
             return await _context.FakePolitikere // Brugt korrekt DbSet navn
                    .OrderBy(p => p.PolitikerNavn)
                    .Select(p => new PoliticianSummaryDto { Id = p.Id, Name = p.PolitikerNavn })
                    .ToListAsync();
        }

        // --- API METODE: Hent dagens citat (RETTET) ---
        public async Task<QuoteDto> GetQuoteOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

             // Hent hele DailySelection objektet for Citat mode i dag
             var selection = await _context.DailySelections
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == GamemodeTypes.Citat);

            // Håndter hvis valget for dagen mangler
            if (selection == null)
            {
                 _logger.LogError("No daily selection found for {GameMode} on {Date} when trying to get quote.", GamemodeTypes.Citat, today);
                 throw new KeyNotFoundException($"Ingen dagens politiker/citat fundet for Citat-mode d. {today}.");
            }

            // Håndter hvis det specifikke citat mangler i databasen (bør ikke ske)
            if (string.IsNullOrEmpty(selection.SelectedQuoteText))
            {
                 _logger.LogWarning("Daily selection for Quote mode on {Date} is missing the selected quote text (PoliticianId: {PoliticianId}).", today, selection.SelectedPolitikerID);
                  throw new InvalidOperationException($"Intet specifikt citat blev gemt for Citat-mode d. {today}.");
            }

            // Returner det gemte citat
            return new QuoteDto { QuoteText = selection.SelectedQuoteText };
        }

        // --- API METODE: Hent dagens foto ---
        public async Task<PhotoDto> GetPhotoOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             int politicianId = await GetSelectedPoliticianIdAsync(GamemodeTypes.Foto, today);

             // Hent kun ID og Portræt for den valgte politiker
             var photoPolitician = await _context.FakePolitikere // Brugt korrekt DbSet navn
                                             .Select(p => new { p.Id, p.Portræt })
                                             .FirstOrDefaultAsync(p => p.Id == politicianId);

            // Håndter hvis politikeren ikke findes
            if (photoPolitician == null)
            {
                 throw new KeyNotFoundException($"Politiker med ID {politicianId} for dagens foto blev ikke fundet.");
            }
            // Håndter hvis portræt-data mangler
            if (photoPolitician.Portræt == null || photoPolitician.Portræt.Length == 0)
            {
                  throw new InvalidOperationException($"Politiker med ID {politicianId} mangler portræt data.");
            }

            // Returner billeddata som Base64-streng
            return new PhotoDto { PortraitBase64 = Convert.ToBase64String(photoPolitician.Portræt) };
        }

        #endregion // API Methods - Data Retrieval

        #region API Methods - Guess Processing

        // --- API METODE: Behandl et gæt fra brugeren ---
        public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
        {
            // Sørg for at 'today' er DateOnly til aldersberegning
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            int targetPoliticianId = await GetSelectedPoliticianIdAsync(guessDto.GameMode, today);

            // Hent både mål-politikeren og den gættede politiker (inkl. partiinfo)
            var politicians = await _context.FakePolitikere // Brugt korrekt DbSet navn
                .Include(p => p.FakeParti) // Vigtigt at få parti med til feedback
                .Where(p => p.Id == targetPoliticianId || p.Id == guessDto.GuessedPoliticianId)
                .ToListAsync();

            var targetPolitician = politicians.FirstOrDefault(p => p.Id == targetPoliticianId);
            var guessedPolitician = politicians.FirstOrDefault(p => p.Id == guessDto.GuessedPoliticianId);

            // Håndter hvis en af politikerne ikke blev fundet
            if (targetPolitician == null) throw new KeyNotFoundException($"Dagens politiker ({targetPoliticianId}) for {guessDto.GameMode} findes ikke i databasen.");
            if (guessedPolitician == null) throw new KeyNotFoundException($"Den gættede politiker ({guessDto.GuessedPoliticianId}) findes ikke i databasen.");

            // Beregn aldre baseret på DateOfBirth
            int targetAge = CalculateAge(DateOnly.FromDateTime(targetPolitician.DateOfBirth), today);
            int guessedAge = CalculateAge(DateOnly.FromDateTime(guessedPolitician.DateOfBirth), today);

            // Byg svar DTO
            var result = new GuessResultDto
            {
                IsCorrectGuess = targetPolitician.Id == guessedPolitician.Id,
                GuessedPolitician = new GuessedPoliticianDetailsDto
                {
                    Id = guessedPolitician.Id,
                    PolitikerNavn = guessedPolitician.PolitikerNavn,
                    PartiNavn = guessedPolitician.FakeParti?.PartiNavn ?? "Ukendt Parti",
                    Age = guessedAge, // Brug den beregnede alder
                    Køn = guessedPolitician.Køn,
                    Uddannelse = guessedPolitician.Uddannelse,
                    Region = guessedPolitician.Region
                }
            };

            // Udfyld detaljeret feedback KUN for Classic mode
            if (guessDto.GameMode == GamemodeTypes.Klassisk)
            {
                result.Feedback = new Dictionary<string, FeedbackType>();
                result.Feedback.Add("Navn", result.IsCorrectGuess ? FeedbackType.Korrekt : FeedbackType.Forkert);
                result.Feedback.Add("Køn", targetPolitician.Køn == guessedPolitician.Køn ? FeedbackType.Korrekt : FeedbackType.Forkert);

                // Alders-feedback baseret på beregnede aldre
                if (targetAge == guessedAge)
                    result.Feedback.Add("Alder", FeedbackType.Korrekt);
                else if (guessedAge < targetAge) // Gættet er YNGRE end målet
                    result.Feedback.Add("Alder", FeedbackType.Højere); // Målet er ÆLDRE (højere alder)
                else // Gættet er ÆLDRE end målet
                    result.Feedback.Add("Alder", FeedbackType.Lavere); // Målet er YNGRE (lavere alder)

                // Feedback for andre attributter
                result.Feedback.Add("Parti", targetPolitician.PartiId == guessedPolitician.PartiId ? FeedbackType.Korrekt : FeedbackType.Forkert);
                result.Feedback.Add("Uddannelse", targetPolitician.Uddannelse == guessedPolitician.Uddannelse ? FeedbackType.Korrekt : FeedbackType.Forkert);
                result.Feedback.Add("Region", targetPolitician.Region == guessedPolitician.Region ? FeedbackType.Korrekt : FeedbackType.Forkert);
            }

            return result; // Returner resultatet
        }

        #endregion // API Methods - Guess Processing

        #region Daily Job Method

        // --- METODE TIL DAGLIGT JOB: Vælg og gem dagens politikere/citat ---
         public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date)
         {
            _logger.LogInformation("Starting daily selection process for {Date}", date);

             // Tjek om valg allerede findes for dagen
             bool alreadyExists = await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
             if (alreadyExists)
             {
                 _logger.LogWarning("Daily selections already exist for {Date}. Skipping generation.", date);
                 return;
             }

             // Hent alle politikere (inkluder citater til Citat-mode valg)
             var allPoliticians = await _context.FakePolitikere // Brugt korrekt DbSet navn
                                        .Include(p => p.Quotes)
                                        .ToListAsync();

             // Håndter hvis ingen politikere findes
             if (!allPoliticians.Any())
             {
                 _logger.LogError("No politicians found in the database. Cannot select daily politicians.");
                 return;
             }

             // Vælg politiker til Classic mode (rent tilfældigt)
             var classicPolitician = allPoliticians[_random.Next(allPoliticians.Count)];

             // Deklarer variabler til Citat og Foto mode FØR if/else blokke
             FakePolitiker? quotePolitician = null;
             FakePolitiker? photoPolitician = null;
             PoliticianQuote? selectedQuote = null; // Til at holde det specifikke citat

             // Vælg politiker OG citat til Citat mode
             var quoteCandidates = allPoliticians.Where(p => p.Quotes != null && p.Quotes.Any()).ToList();
              if (!quoteCandidates.Any()) // Fallback hvis ingen har citater
              {
                  quotePolitician = allPoliticians[_random.Next(allPoliticians.Count)];
                  _logger.LogWarning("No politicians with quotes found. Selected random politician {PoliticianId} for Quote mode. No specific quote selected.", quotePolitician.Id);
                  // selectedQuote forbliver null
              }
              else // Vælg blandt dem med citater
              {
                 quotePolitician = quoteCandidates[_random.Next(quoteCandidates.Count)];
                 // Vælg ét tilfældigt citat fra den valgte politiker
                 selectedQuote = quotePolitician.Quotes.ElementAt(_random.Next(quotePolitician.Quotes.Count));
                 _logger.LogInformation("Selected politician {PoliticianId} and quote {QuoteId} for Quote mode.", quotePolitician.Id, selectedQuote.QuoteId);
              }

             // Vælg politiker til Foto mode
             var photoCandidates = allPoliticians.Where(p => p.Portræt != null && p.Portræt.Length > 0).ToList();
              if (!photoCandidates.Any()) // Fallback hvis ingen har portræt
              {
                  photoPolitician = allPoliticians[_random.Next(allPoliticians.Count)];
                  _logger.LogWarning("No politicians with portraits found. Selected random politician {PoliticianId} for Photo mode.", photoPolitician.Id);
              }
              else // Vælg blandt dem med portræt
              {
                  photoPolitician = photoCandidates[_random.Next(photoCandidates.Count)];
              }

             // Sikkerhedstjek før gemning
             if (classicPolitician == null || quotePolitician == null || photoPolitician == null)
             {
                 _logger.LogError("Could not select politicians for all gamemodes for {Date}. Aborting save.", date);
                 return;
             }

              // Opret DailySelection objekter (inkl. det valgte citat for Citat-mode)
              var dailySelections = new List<DailySelection>
              {
                  new DailySelection {
                      SelectionDate = date,
                      GameMode = GamemodeTypes.Klassisk,
                      SelectedPolitikerID = classicPolitician.Id
                      // SelectedQuoteText er null her
                  },
                  new DailySelection {
                      SelectionDate = date,
                      GameMode = GamemodeTypes.Citat,
                      SelectedPolitikerID = quotePolitician.Id,
                      SelectedQuoteText = selectedQuote?.QuoteText // Gem teksten hvis et citat blev valgt
                  },
                   new DailySelection {
                      SelectionDate = date,
                      GameMode = GamemodeTypes.Foto,
                      SelectedPolitikerID = photoPolitician.Id
                      // SelectedQuoteText er null her
                  }
              };

             // Gem de nye daglige valg i databasen
             await _context.DailySelections.AddRangeAsync(dailySelections);
             await _context.SaveChangesAsync();

             // Log success
             _logger.LogInformation("Successfully selected and saved daily politicians for {Date}. Classic: {ClassicId}, Quote: {QuoteId}, Photo: {PhotoId}",
                date, classicPolitician.Id, quotePolitician.Id, photoPolitician.Id);
        }

        #endregion // Daily Job Method
    }
}