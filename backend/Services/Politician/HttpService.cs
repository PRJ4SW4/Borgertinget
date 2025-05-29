using System.Text.Json;

namespace backend.Services.Politicians
{
    public class HttpService
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<T?> GetJsonAsync<T>(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(jsonResponse);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
