using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc; // For IActionResult if detailed results are needed

namespace backend.Services.Politicians
{
    public interface IFetchService
    {
        Task<(int totalAdded, int totalUpdated, int totalDeleted)> FetchAndUpdateAktorsAsync();
    }
}
