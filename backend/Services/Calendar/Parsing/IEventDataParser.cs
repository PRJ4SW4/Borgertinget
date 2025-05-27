namespace backend.Services.Calendar.Parsing;

using System.Collections.Generic;
using backend.Models.Calendar;

// Defines a contract for services that can parse HTML content into a list of ScrapedEventData objects.
public interface IEventDataParser
{
    // Parses the provided HTML content and extracts event data.
    // Returns a list of ScrapedEventData objects.
    List<ScrapedEventData> ParseEvents(string htmlContent);
}
