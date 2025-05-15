namespace backend.Services.Search
{
    public interface ISearchIndexService
    {
        Task RunFullIndexAsync(CancellationToken cancellationToken);
    }
}
