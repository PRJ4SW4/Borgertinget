namespace backend.Services.AutomationServices;

public interface IAltingetScraperService
{
    Task<List<ScrapedEventData>> ScrapeEventsAsyncInternal();
    Task<int> RunAutomation();
}
