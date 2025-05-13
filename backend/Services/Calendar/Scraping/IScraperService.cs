namespace backend.Services.Calendar.Scraping;

public interface IScraperService
{
    Task<int> RunScraper();
}
