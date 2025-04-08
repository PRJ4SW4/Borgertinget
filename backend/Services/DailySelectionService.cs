
namespace backend.Services; // Eller dit service-namespace

using backend.Data; // Dit DataContext namespace
using backend.Models; // Dit Models namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Tilføj logging
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IDailySelectionService
{
    Task<FakePolitiker?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode);
}

public class DailySelectionService : IDailySelectionService
{
    private readonly DataContext _context;
    private readonly ILogger<DailySelectionService> _logger;
    private static readonly Random _random = new Random(); // Genbrug Random instans

    public DailySelectionService(DataContext context, ILogger<DailySelectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FakePolitiker?> GetOrSelectDailyPoliticianAsync(GamemodeTypes gameMode)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow); // Brug UTC dato

        // 1. Tjek om dagens valg allerede findes i databasen
        var existingSelection = await _context.DailySelections
                                    .Include(ds => ds.SelectedPolitiker) // Inkluder politiker data
                                    .FirstOrDefaultAsync(ds => ds.SelectionDate == today && ds.GameMode == gameMode);

        if (existingSelection != null)
        {
            _logger.LogInformation("Found existing daily selection for {GameMode} on {Date}: PolitikerId {PolitikerId}", gameMode, today, existingSelection.SelectedPolitikerID);
            return existingSelection.SelectedPolitiker;
        }

        // 2. Hvis ikke fundet, udfør vægtet udvælgelse
        _logger.LogInformation("No existing selection found for {GameMode} on {Date}. Performing weighted selection.", gameMode, today);

        // Lås for at forhindre race conditions hvis flere requests rammer samtidigt lige omkring midnat
        // Overvej en mere robust distribueret lås i et produktionsmiljø
        // lock (_lockObject) // Kræver definition af et static object _lockObject
        // {
            // Dobbelt-tjek om en anden tråd lige har lavet valget, mens vi ventede på låsen
            // existingSelection = await _context.DailySelections... (gentag query fra trin 1)
            // if (existingSelection != null) return existingSelection.SelectedPolitiker;

            try
            {
                var selectedPolitician = await PerformWeightedSelectionAsync(gameMode, today);

                if (selectedPolitician == null)
                {
                    _logger.LogWarning("Weighted selection for {GameMode} returned no politician.", gameMode);
                    return null; // Kunne ikke vælge en
                }

                // 3. Gem det nye valg og opdater tracking
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Opret nyt dagligt valg
                    var newDailySelection = new DailySelection
                    {
                        SelectionDate = today,
                        GameMode = gameMode,
                        SelectedPolitikerID = selectedPolitician.Id,
                        // SelectedPolitiker = selectedPolitician // Ikke nødvendig at sætte her
                    };
                    _context.DailySelections.Add(newDailySelection);

                    // Opdater LastSelectedDate for den valgte politiker i den specifikke gamemode
                    var trackingEntry = await _context.GameTrackings
                                            .FirstOrDefaultAsync(gt => gt.PolitikerId == selectedPolitician.Id && gt.GameMode == gameMode);

                    if (trackingEntry != null)
                    {
                        trackingEntry.LastSelectedDate = today;
                        // AlgoWeight skal ikke sættes her, da den beregnes dynamisk
                    }
                    else
                    {
                        // Burde ikke ske hvis initialisering er korrekt, men håndter det evt.
                        _logger.LogWarning("Could not find tracking entry for PolitikerId {PolitikerId} and GameMode {GameMode} to update LastSelectedDate.", selectedPolitician.Id, gameMode);
                        // Opret evt. tracking entry her hvis den mangler?
                    }

                    await _context.SaveChangesAsync(); // Gem ændringer (både DailySelection og GameTracking)
                    await transaction.CommitAsync(); // Gennemfør transaktionen

                    _logger.LogInformation("Successfully selected and saved PolitikerId {PolitikerId} for {GameMode} on {Date}", selectedPolitician.Id, gameMode, today);

                    return selectedPolitician;
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
                _logger.LogError(ex, "Error during weighted selection process for {GameMode}.", gameMode);
                return null; // Eller kast fejlen videre
            }
        // } // End lock
    }


    private async Task<FakePolitiker?> PerformWeightedSelectionAsync(GamemodeTypes gameMode, DateOnly today)
    {
            const int maxWeightDays = 365 * 2; // Sæt en øvre grænse for vægt for at undgå ekstremt store tal
            const int defaultWeight = maxWeightDays + 1; // Vægt hvis aldrig valgt (højere end max)

        // Hent alle aktive politikere og deres tracking info for den givne gamemode
        var candidates = await _context.FakePolitikere
            // .Where(p => p.IsActive) // Tilføj IsActive property til FakePolitiker hvis relevant
            .Select(p => new
            {
                Politician = p,
                Tracking = _context.GameTrackings
                                .Where(gt => gt.PolitikerId == p.Id && gt.GameMode == gameMode)
                                .FirstOrDefault() // Få den ene tracking record for denne gamemode
            })
            .ToListAsync();

        if (!candidates.Any())
        {
            _logger.LogWarning("No candidates found for weighted selection for {GameMode}.", gameMode);
            return null;
        }

        var weightedCandidates = candidates.Select(c => {
                int daysSinceLast = c.Tracking?.LastSelectedDate.HasValue ?? false
                ? today.DayNumber - c.Tracking.LastSelectedDate.Value.DayNumber // Antal dage siden sidst valgt
                : defaultWeight; // Aldrig valgt = default høj vægt

            // Sørg for at vægten er mindst 1 og ikke over max
            int weight = Math.Max(1, Math.Min(daysSinceLast, maxWeightDays));

            // Juster evt. vægtformel her (f.eks. weight = weight * weight;)
            return new { c.Politician, Weight = weight };
        }).ToList();


        long totalWeight = weightedCandidates.Sum(c => (long)c.Weight); // Brug long for at undgå overflow

        if (totalWeight <= 0)
        {
                _logger.LogWarning("Total weight is zero or negative for {GameMode}. Cannot perform weighted selection. Selecting purely random.", gameMode);
                // Vælg rent tilfældigt som fallback
                int randomIndex = _random.Next(weightedCandidates.Count);
                return weightedCandidates[randomIndex].Politician;
        }

        // Vægtet udvælgelse (Kumulativ vægt metode)
        long randomValue = (long)(_random.NextDouble() * totalWeight); // Tilfældigt tal mellem 0 og totalWeight
        long cumulativeWeight = 0;

        foreach (var candidate in weightedCandidates)
        {
            cumulativeWeight += candidate.Weight;
            if (randomValue < cumulativeWeight)
            {
                _logger.LogDebug("Selected {PolitikerId} with weight {Weight} for {GameMode} based on random value {RandomValue} (Total Weight: {TotalWeight})",
                    candidate.Politician.Id, candidate.Weight, gameMode, randomValue, totalWeight);
                return candidate.Politician;
            }
        }

        // Fallback (burde ikke rammes hvis totalWeight > 0)
        _logger.LogWarning("Weighted selection fallback triggered for {GameMode}. Selecting last candidate.", gameMode);
        return weightedCandidates.LastOrDefault()?.Politician;
    }
}