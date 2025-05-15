namespace backend.Services.Politician
{
    public interface IHttpService
    {
        Task<T?> GetJsonAsync<T>(string url);
    }
}
