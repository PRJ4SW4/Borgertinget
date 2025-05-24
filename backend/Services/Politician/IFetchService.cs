using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Politicians
{
    public interface IFetchService
    {
        Task<(int totalAdded, int totalUpdated, int totalDeleted)> FetchAndUpdateAktorsAsync();
    }
}
