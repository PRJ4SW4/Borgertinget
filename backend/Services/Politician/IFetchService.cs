namespace backend.Services.Politicians
{
    public interface IFetchService
    {
        Task<(int totalAdded, int totalUpdated, int totalDeleted)> FetchAndUpdateAktorsAsync();
    }
}
