using Microsoft.AspNetCore.Mvc; // For IActionResult if detailed results are needed
using System.Threading.Tasks;

namespace backend.Services.Politician{
    public interface IFetchService{
        Task<(int totalAdded, int totalUpdated, int totalDeleted)> FetchAndUpdateAktorsAsync();
    }
}

