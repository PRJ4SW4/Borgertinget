// DailySelectionService.cs
using backend.Models;
using backend.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data; // Sørg for using til DataContext

namespace backend.Services
{
    public class DailySelectionService : IDailySelectionService
    {
        private readonly DataContext _context;
        private readonly ILogger<DailySelectionService> _logger;
        private readonly Random _random = new Random();

        public DailySelectionService(DataContext context, ILogger<DailySelectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- Aldersberegning (uændret) ---
        private int CalculateAge(DateOnly dateOfBirth, DateOnly referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.DayOfYear < dateOfBirth.DayOfYear)
            {
                age--;
            }
            return Math.Max(0, age);
        }

        // --- GetSelectedPoliticianIdAsync (uændret) ---
        private async Task<int> GetSelectedPoliticianIdAsync(GamemodeTypes gameMode, DateOnly today)
        {
             var selection = await _context.DailySelections
                .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);

            if (selection == null)
            {
                 _logger.LogError("No daily selection found for {GameMode} on {Date}. Run the daily selection job.", gameMode, today);
                 throw new KeyNotFoundException($"Ingen dagens politiker fundet for {gameMode} d. {today}.");
            }
            return selection.SelectedPolitikerID;
        }

        // --- GetAllPoliticiansForGuessingAsync (uændret fra sidst) ---
        public async Task<List<PoliticianSummaryDto>> GetAllPoliticiansForGuessingAsync()
        {
             return await _context.FakePolitikere // Korrekt DbSet navn
                    .OrderBy(p => p.PolitikerNavn)
                    .Select(p => new PoliticianSummaryDto { Id = p.Id, Name = p.PolitikerNavn })
                    .ToListAsync();
        }

        // --- GetQuoteOfTheDayAsync (uændret fra sidst) ---
         public async Task<QuoteDto> GetQuoteOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             int politicianId = await GetSelectedPoliticianIdAsync(GamemodeTypes.Citat, today);

             var quotePolitician = await _context.FakePolitikere // Korrekt DbSet navn
                                         .Include(p => p.Quotes)
                                         .FirstOrDefaultAsync(p => p.Id == politicianId);

             if (quotePolitician == null) { throw new KeyNotFoundException($"Politiker med ID {politicianId} for dagens citat blev ikke fundet."); }
              if (quotePolitician.Quotes == null || !quotePolitician.Quotes.Any()) { throw new InvalidOperationException($"Politiker {quotePolitician.PolitikerNavn} har ingen citater tilknyttet."); }

              var quote = quotePolitician.Quotes.ElementAt(_random.Next(quotePolitician.Quotes.Count));
              return new QuoteDto { QuoteText = quote.QuoteText };
        }

        // --- GetPhotoOfTheDayAsync (uændret fra sidst) ---
        public async Task<PhotoDto> GetPhotoOfTheDayAsync()
        {
             DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
             int politicianId = await GetSelectedPoliticianIdAsync(GamemodeTypes.Foto, today);

             var photoPolitician = await _context.FakePolitikere // Korrekt DbSet navn
                                             .Select(p => new { p.Id, p.Portræt })
                                             .FirstOrDefaultAsync(p => p.Id == politicianId);

            if (photoPolitician == null) { throw new KeyNotFoundException($"Politiker med ID {politicianId} for dagens foto blev ikke fundet."); }
             if (photoPolitician.Portræt == null || photoPolitician.Portræt.Length == 0) { throw new InvalidOperationException($"Politiker med ID {politicianId} mangler portræt data."); }

            return new PhotoDto { PortraitBase64 = Convert.ToBase64String(photoPolitician.Portræt) };
        }


        // --- ProcessGuessAsync (RETTET ved 'today' definition) ---
        public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
        {
            // RETTET: Sørg for at 'today' er DateOnly
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            int targetPoliticianId = await GetSelectedPoliticianIdAsync(guessDto.GameMode, today);

            var politicians = await _context.FakePolitikere // Korrekt DbSet navn
                .Include(p => p.FakeParti)
                .Where(p => p.Id == targetPoliticianId || p.Id == guessDto.GuessedPoliticianId)
                .ToListAsync();

            var targetPolitician = politicians.FirstOrDefault(p => p.Id == targetPoliticianId);
            var guessedPolitician = politicians.FirstOrDefault(p => p.Id == guessDto.GuessedPoliticianId);

            if (targetPolitician == null) throw new KeyNotFoundException($"Dagens politiker ({targetPoliticianId}) for {guessDto.GameMode} findes ikke i databasen.");
            if (guessedPolitician == null) throw new KeyNotFoundException($"Den gættede politiker ({guessDto.GuessedPoliticianId}) findes ikke i databasen.");

            // Kald til CalculateAge bruger nu den korrekte type for 'today'
            int targetAge = CalculateAge(DateOnly.FromDateTime(targetPolitician.DateOfBirth), today);
            int guessedAge = CalculateAge(DateOnly.FromDateTime(guessedPolitician.DateOfBirth), today);

            var result = new GuessResultDto
            {
                IsCorrectGuess = targetPolitician.Id == guessedPolitician.Id,
                GuessedPolitician = new GuessedPoliticianDetailsDto
                {
                    Id = guessedPolitician.Id,
                    PolitikerNavn = guessedPolitician.PolitikerNavn,
                    PartiNavn = guessedPolitician.FakeParti?.PartiNavn ?? "Ukendt Parti",
                    Age = guessedAge,
                    Køn = guessedPolitician.Køn,
                    Uddannelse = guessedPolitician.Uddannelse,
                    Region = guessedPolitician.Region
                }
            };

            if (guessDto.GameMode == GamemodeTypes.Klassisk)
            {
                result.Feedback = new Dictionary<string, FeedbackType>();
                result.Feedback.Add("Navn", result.IsCorrectGuess ? FeedbackType.Korrekt : FeedbackType.Forkert);
                result.Feedback.Add("Køn", targetPolitician.Køn == guessedPolitician.Køn ? FeedbackType.Korrekt : FeedbackType.Forkert);

                if (targetAge == guessedAge)
                    result.Feedback.Add("Alder", FeedbackType.Korrekt);
                else if (guessedAge < targetAge)
                    result.Feedback.Add("Alder", FeedbackType.Højere);
                else
                    result.Feedback.Add("Alder", FeedbackType.Lavere);

                result.Feedback.Add("Parti", targetPolitician.PartiId == guessedPolitician.PartiId ? FeedbackType.Korrekt : FeedbackType.Forkert);
                result.Feedback.Add("Uddannelse", targetPolitician.Uddannelse == guessedPolitician.Uddannelse ? FeedbackType.Korrekt : FeedbackType.Forkert);
                result.Feedback.Add("Region", targetPolitician.Region == guessedPolitician.Region ? FeedbackType.Korrekt : FeedbackType.Forkert);
            }

            return result;
        }


        // --- SelectAndSaveDailyPoliticiansAsync (RETTET variabel scope) ---
         public async Task SelectAndSaveDailyPoliticiansAsync(DateOnly date)
         {
            _logger.LogInformation("Starting daily selection process for {Date}", date);

             bool alreadyExists = await _context.DailySelections.AnyAsync(ds => ds.SelectionDate == date);
             if (alreadyExists)
             {
                 _logger.LogWarning("Daily selections already exist for {Date}. Skipping generation.", date);
                 return;
             }

             var allPoliticians = await _context.FakePolitikere // Korrekt DbSet navn
                                        .Include(p => p.Quotes)
                                        .ToListAsync();

             if (!allPoliticians.Any())
             {
                 _logger.LogError("No politicians found in the database. Cannot select daily politicians.");
                 return;
             }

             // Vælg for Classic
             var classicPolitician = allPoliticians[_random.Next(allPoliticians.Count)];

             // RETTET: Deklarer variablerne FØR if/else
             FakePolitiker? quotePolitician = null; // Brug nullable (?) hvis der er en chance for at de ikke bliver sat
             FakePolitiker? photoPolitician = null;

             // Vælg for Citat
             var quoteCandidates = allPoliticians.Where(p => p.Quotes != null && p.Quotes.Any()).ToList();
              if (!quoteCandidates.Any())
              {
                  // Tildel til den ydre variabel
                  quotePolitician = allPoliticians[_random.Next(allPoliticians.Count)];
                  _logger.LogWarning("No politicians with quotes found. Selected random politician {PoliticianId} for Quote mode.", quotePolitician.Id);
              }
              else
              {
                 // Tildel til den ydre variabel
                 quotePolitician = quoteCandidates[_random.Next(quoteCandidates.Count)];
              }

             // Vælg for Foto
             var photoCandidates = allPoliticians.Where(p => p.Portræt != null && p.Portræt.Length > 0).ToList();
              if (!photoCandidates.Any())
              {
                   // Tildel til den ydre variabel
                  photoPolitician = allPoliticians[_random.Next(allPoliticians.Count)];
                  _logger.LogWarning("No politicians with portraits found. Selected random politician {PoliticianId} for Photo mode.", photoPolitician.Id);
              }
              else
              {
                  // Tildel til den ydre variabel
                  photoPolitician = photoCandidates[_random.Next(photoCandidates.Count)];
              }

             // Sikkerhedstjek før brug - hvis en politiker af en eller anden grund ikke kunne vælges
             if (classicPolitician == null || quotePolitician == null || photoPolitician == null)
             {
                 _logger.LogError("Could not select politicians for all gamemodes for {Date}. Aborting save.", date);
                 return; // Undlad at gemme hvis noget gik galt
             }

              // --- Gem Valgene ---
              // Variablerne er nu kendt her
              var dailySelections = new List<DailySelection>
              {
                  // Brug .Id fra de nu korrekt scopede variabler
                  new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Klassisk, SelectedPolitikerID = classicPolitician.Id },
                  new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Citat, SelectedPolitikerID = quotePolitician.Id },
                  new DailySelection { SelectionDate = date, GameMode = GamemodeTypes.Foto, SelectedPolitikerID = photoPolitician.Id }
              };

             await _context.DailySelections.AddRangeAsync(dailySelections);
             await _context.SaveChangesAsync();

             // Logningen bruger nu også de korrekt scopede variabler
             _logger.LogInformation("Successfully selected and saved daily politicians for {Date}. Classic: {ClassicId}, Quote: {QuoteId}, Photo: {PhotoId}",
                date, classicPolitician.Id, quotePolitician.Id, photoPolitician.Id);
        }
    }
}