using backend.Models;
using System.Collections.Generic;


namespace backend.Interfaces.Services
{
    // Input til algoritmen: Politiker og evt. dens tracker for den relevante gamemode
    public record CandidateData(Aktor Politician, GamemodeTracker? Tracker);

    public interface ISelectionAlgorithm
    {
        /// <summary>
        /// Vælger en kandidat fra listen baseret på en vægtet, tilfældig algoritme.
        /// </summary>
        /// <param name="candidates">Listen af mulige kandidater med deres tracker data.</param>
        /// <param name="currentDate">Datoen udvælgelsen sker for (bruges til vægtberegning).</param>
        /// <param name="gameMode">Den aktuelle gamemode (kan påvirke vægtning).</param>
        /// <returns>Den valgte Aktor, eller null hvis ingen kunne vælges.</returns>
        Aktor? SelectWeightedRandomCandidate(IEnumerable<CandidateData> candidates, DateOnly currentDate, GamemodeTypes gameMode);
    }
}