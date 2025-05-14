using System;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace backend.Services.Politician
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
                Console.WriteLine("Raw JSON Response:");
                Console.WriteLine(jsonResponse);  // Debugging output

                return JsonSerializer.Deserialize<T>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                return default;
            }
        }
    }
}
