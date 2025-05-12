namespace backend.Services.AutomationServices.HtmlFetching;

// Defines a contract for services that can fetch HTML content from a specified URL.
public interface IHtmlFetcher
{
    // Asynchronously fetches HTML content from the given URL.
    // Returns the HTML content as a string, or null if fetching fails.
    Task<string?> FetchHtmlAsync(string url);
}
