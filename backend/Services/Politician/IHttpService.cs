namespace backend.Services.Politicians
{
    public interface IHttpService
    {
        Task<T?> GetJsonAsync<T>(string url);
    }
}
