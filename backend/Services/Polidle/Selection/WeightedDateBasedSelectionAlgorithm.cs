using backend.Interfaces.Repositories; // Hvis den skal bruge tracker repo
using backend.Interfaces.Services;
using backend.Interfaces.Utility;
using backend.Models;
using backend.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.Selection
{
    public class WeightedDateBasedSelectionAlgorithm : ISelectionAlgorithm
    {
        private readonly ILogger<WeightedDateBasedSelectionAlgorithm> _logger;
        private readonly IRandomProvider _randomProvider;

        public WeightedDateBasedSelectionAlgorithm(ILogger<WeightedDateBasedSelectionAlgorithm> logger, IRandomProvider randomProvider)
        {
            _logger = logger;
            _randomProvider = randomProvider;
        }

        public Aktor? SelectWeightedRandomCandidate(IEnumerable<CandidateData> candidates, DateOnly currentDate, GamemodeTypes gameMode)
        {
             if (candidates == null || !candidates.Any())
             {
                 _logger.LogWarning("SelectWeightedRandomCandidate called with no candidates for {GameMode} on {Date}.", gameMode, currentDate);
                 return null;
             }

             // Beregn vægt for hver kandidat baseret på tracker data
             var weightedCandidates = candidates
                 .Select(c => (politician: c.Politician, weight: CalculateSelectionWeight(c.Tracker, currentDate)))
                 .Where(c => c.weight > 0) // Kun dem med positiv vægt kan vælges
                 .ToList();


             if (!weightedCandidates.Any())
             {
                 _logger.LogWarning("No valid candidates with positive weight found for {GameMode} on {Date}. Falling back to random unweighted selection.", gameMode, currentDate);
                  // Fallback: Vælg tilfældigt blandt *alle* oprindelige kandidater
                 var originalCandidates = candidates.Select(c => c.Politician).ToList();
                  if (!originalCandidates.Any()) return null;
                  return originalCandidates[_randomProvider.Next(originalCandidates.Count)];
             }

            // --- Vægtet tilfældig udvælgelse ---
            long totalWeight = weightedCandidates.Sum(c => (long)c.weight);
            if (totalWeight <= 0)
            {
                 _logger.LogWarning("Total weight is zero or negative for {GameMode} on {Date}. Falling back to random selection among weighted candidates.", gameMode, currentDate);
                 return weightedCandidates[_randomProvider.Next(weightedCandidates.Count)].politician; // Vælg tilfældigt blandt de filtrerede
            }

            double randomValue = _randomProvider.NextDouble() * totalWeight;
            long cumulativeWeight = 0;

            foreach (var candidate in weightedCandidates)
            {
                cumulativeWeight += candidate.weight;
                if (randomValue < cumulativeWeight)
                {
                    _logger.LogDebug("Selected candidate {AktorId} with weight {Weight} for {GameMode} on {Date}.", candidate.politician.Id, candidate.weight, gameMode, currentDate);
                    return candidate.politician;
                }
            }

            // Fallback (bør sjældent rammes)
            _logger.LogError("Weighted random selection failed unexpectedly for {GameMode} on {Date}. Returning last valid candidate.", gameMode, currentDate);
            return weightedCandidates.LastOrDefault().politician;
        }


        private int CalculateSelectionWeight(GamemodeTracker? tracker, DateOnly currentDate)
        {
            // Giver højere vægt jo længere tid siden politikeren sidst blev valgt i denne gamemode.
            const int maxWeightDays = 365; // Max vægt svarer til 1 år siden sidst valgt
            const int defaultWeight = maxWeightDays + 1; // Vægt hvis aldrig valgt før
            const int baseWeightIfChosen = 1; // Minimum vægt hvis valgt før

            if (tracker?.LastSelectedDate == null)
            {
                return defaultWeight; // Aldrig valgt -> høj vægt
            }
            else
            {
                int daysSinceLast = currentDate.DayNumber - tracker.LastSelectedDate.Value.DayNumber;
                // Vægt stiger med antal dage siden sidst, op til maxWeightDays. Minimum 1.
                return Math.Max(baseWeightIfChosen, Math.Min(maxWeightDays, daysSinceLast));
            }
        }
    }
}