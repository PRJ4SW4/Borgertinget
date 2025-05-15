using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Interfaces.Repositories
{
    public interface IAktorRepository
    {
        Task<Aktor?> GetByIdAsync(int id, bool includeParty = false);
        Task<List<Aktor>> GetAllForSummaryAsync(string? search = null, int maxResults = 15);
        Task<List<Aktor>> GetAllWithDetailsForSelectionAsync(); // Henter alle med nødvendige includes for daglig valg
    }
}
