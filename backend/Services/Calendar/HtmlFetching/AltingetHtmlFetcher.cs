namespace backend.Services.Calendar.HtmlFetching;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// Fetches HTML content specifically from Altinget, using a custom User-Agent.
public class AltingetHtmlFetcher : IHtmlFetcher
{
    private readonly IHttpClientFactory _httpClientFactory; // Factory for creating HTTP clients.
    private readonly ILogger<AltingetHtmlFetcher> _logger; // Logger for recording activity and errors.
    private const string CustomUserAgent =
        "MyBorgertingetCalendarBot/1.0 (+http://borgertinget/botinfo)"; // Custom User-Agent string for HTTP requests.

    public AltingetHtmlFetcher(
        IHttpClientFactory httpClientFactory,
        ILogger<AltingetHtmlFetcher> logger
    )
    {
        _httpClientFactory =
            httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Asynchronously fetches HTML content from the given URL.
    public async Task<string?> FetchHtmlAsync(string url)
    {
        var httpClient = _httpClientFactory.CreateClient("AltingetFetcherClient"); // Create an HTTP client using the factory.
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(CustomUserAgent); // Set the custom User-Agent header. Lets Altinget block us if we are doing harm.
        string? htmlContent = null; // Variable to store the HTML content.

        try
        {
            _logger.LogDebug("Attempting to fetch HTML from URL: {Url}", url);
            HttpResponseMessage response = await httpClient.GetAsync(url); // Send an HTTP GET request.
            response.EnsureSuccessStatusCode(); // Ensure the response status code indicates success.
            htmlContent = await response.Content.ReadAsStringAsync(); // Read the HTML content.
            _logger.LogDebug(
                "Successfully fetched HTML content ({Length} bytes) from URL: {Url}",
                htmlContent.Length,
                url
            );
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(
                e,
                "Error fetching HTML content from URL {Url}: {ErrorMessage}",
                url,
                e.Message
            ); // Log an error if the request fails.
            // htmlContent remains null
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred while fetching HTML from URL {Url}",
                url
            );
            // htmlContent remains null
        }
        return htmlContent; // Return the HTML content, or null if an error occurred.
    }
}
