using backend.Enums;
using backend.Models;
using backend.Models.Politicians;

namespace backend.Interfaces.Services
{
    public record CandidateData(Aktor Politician, GamemodeTracker? Tracker);

    public interface ISelectionAlgorithm
    {
        Aktor? SelectWeightedRandomCandidate(
            IEnumerable<CandidateData> candidates,
            DateOnly currentDate,
            GamemodeTypes gameMode
        );
    }
}
