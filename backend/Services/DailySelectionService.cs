// Fil: DailySelectionService.cs
namespace backend.Services; // Eller dit service-namespace

using backend.Data;
using backend.Models;
using backend.DTO; // Tilføj for DTOs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Klassen implementerer nu interfacet (som ligger i sin egen fil)
public class DailySelectionService : IDailySelectionService
{
    private readonly DataContext _context;
    private readonly ILogger<DailySelectionService> _logger;
    private static readonly Random _random = new Random();

    public DailySelectionService(DataContext context, ILogger<DailySelectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Din eksisterende GetOrSelectDailyPoliticianAsync metode...
    public async Task<FakePolitiker?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode)
    {
         // ... (Implementering som du har den) ...
         // Sørg for at inkludere FakeParti her!
         var today = DateOnly.FromDateTime(DateTime.UtcNow);
         var existingSelection = await _context.DailySelections
             .Include(ds => ds.SelectedPolitiker)
                 .ThenInclude(p => p.FakeParti) // VIGTIGT: Inkluder parti
             .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);

         if (existingSelection?.SelectedPolitiker != null)
         {
             _logger.LogInformation("Found existing daily selection for {GameMode} on {Date}: PolitikerId {PolitikerId}", gameMode, today, existingSelection.SelectedPolitikerID);
             return existingSelection.SelectedPolitiker;
         }
         // ... resten af din udvælgelseslogik ...
          _logger.LogInformation("No existing selection found for {GameMode} on {Date}. Performing weighted selection.", gameMode, today);
         // Implementer PerformWeightedSelectionAsync eller kald den her
         // return await PerformWeightedSelectionAsync(gameMode, today); // Eksempel
          return null; // Returner null indtil implementeret
    }


    // Implementering af ProcessGuessAsync (fra forrige svar)
    public async Task<GuessResultDto> ProcessGuessAsync(GuessRequestDto guessDto)
    {
        _logger.LogInformation("Processing guess for GameMode {GameMode}, GuessedId {GuessedId}", guessDto.GameMode, guessDto.GuessedPoliticianId);

        // 1. Hent dagens korrekte politiker (via GetOrSelectDailyPoliticianAsync)
        //    SØRG FOR AT GetOrSelectDailyPoliticianAsync INKLUDERER PARTI!
        var correctPolitician = await GetOrSelectDailyPoliticianAsync(guessDto.GameMode);
        if (correctPolitician == null)
        {
             _logger.LogError("Could not find correct daily politician for GameMode {GameMode} to process guess.", guessDto.GameMode);
             throw new KeyNotFoundException($"Ingen dagens politiker fundet for spiltype {guessDto.GameMode}.");
        }
         // Dobbelttjek parti-info (kan fjernes hvis GetOrSelect altid inkluderer det)
        if (correctPolitician.FakeParti == null && correctPolitician.PartiId > 0) {
             correctPolitician.FakeParti = await _context.FakePartier.FindAsync(correctPolitician.PartiId);
             _logger.LogWarning("Manually loaded missing party for correct politician {PoliticianId}", correctPolitician.Id);
        }


        // 2. Hent den gættede politiker (inkluder parti!)
        var guessedPolitician = await _context.FakePolitikere
                                        .Include(p => p.FakeParti) // Inkluder parti-data!
                                        .FirstOrDefaultAsync(p => p.Id == guessDto.GuessedPoliticianId);
        if (guessedPolitician == null)
        {
             _logger.LogError("Could not find guessed politician with ID {GuessedId}.", guessDto.GuessedPoliticianId);
             throw new KeyNotFoundException($"Den gættede politiker med ID {guessDto.GuessedPoliticianId} blev ikke fundet.");
        }

         _logger.LogInformation("Comparing Guessed: {GuessedName} (Party: {GuessedParty}) with Correct: {CorrectName} (Party: {CorrectParty})",
            guessedPolitician.PolitikerNavn, guessedPolitician.FakeParti?.PartiNavn ?? "N/A",
            correctPolitician.PolitikerNavn, correctPolitician.FakeParti?.PartiNavn ?? "N/A");


        // 3. Byg resultat DTO
        var result = new GuessResultDto { /* ... som i forrige svar ... */ };
         result.IsCorrectGuess = correctPolitician.Id == guessedPolitician.Id;
         result.Feedback = new Dictionary<string, FeedbackType>();
         result.GuessedPolitician = new GuessedPoliticianDetailsDto
        {
             Id = guessedPolitician.Id,
             PolitikerNavn = guessedPolitician.PolitikerNavn,
             PartiNavn = guessedPolitician.FakeParti?.PartiNavn ?? "Ukendt Parti",
             Alder = guessedPolitician.Alder,
             Køn = guessedPolitician.Køn,
             Uddannelse = guessedPolitician.Uddannelse,
             Region = guessedPolitician.Region
        };


        // 4. Udfør sammenligninger og udfyld Feedback
        result.Feedback["Navn"] = result.IsCorrectGuess ? FeedbackType.Korrekt : FeedbackType.Forkert;
        result.Feedback["Parti"] = correctPolitician.PartiId == guessedPolitician.PartiId ? FeedbackType.Korrekt : FeedbackType.Forkert;
         if (correctPolitician.Alder == guessedPolitician.Alder)
            result.Feedback["Alder"] = FeedbackType.Korrekt;
        else if (correctPolitician.Alder > guessedPolitician.Alder)
            result.Feedback["Alder"] = FeedbackType.Højere;
        else
            result.Feedback["Alder"] = FeedbackType.Lavere;
         result.Feedback["Region"] = string.Equals(correctPolitician.Region, guessedPolitician.Region, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
        result.Feedback["Køn"] = string.Equals(correctPolitician.Køn, guessedPolitician.Køn, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;
        result.Feedback["Uddannelse"] = string.Equals(correctPolitician.Uddannelse, guessedPolitician.Uddannelse, StringComparison.OrdinalIgnoreCase) ? FeedbackType.Korrekt : FeedbackType.Forkert;


        _logger.LogInformation("Guess result calculated for GuessedId {GuessedId}: IsCorrect={IsCorrect}, Feedback={FeedbackCount}",
            guessDto.GuessedPoliticianId, result.IsCorrectGuess, result.Feedback.Count);

        return result;
    }

    // Din private PerformWeightedSelectionAsync metode...
    private async Task<FakePolitiker?> PerformWeightedSelectionAsync(GamemodeTypes gameMode, DateOnly today)
    {
        // ... (Implementering som du har den) ...
         return null; // Returner null indtil implementeret
    }
}