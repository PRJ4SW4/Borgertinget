using backend.utils.TimeZone;

public class TimeZoneHelper : ITimeZoneHelper
{
    private readonly ILogger<TimeZoneHelper> _logger;

    // Constructor to inject ILogger
    public TimeZoneHelper(ILogger<TimeZoneHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Helper to find the timezone reliably on different OS
    // This makes sure it will work on both Linux, MacOS and Windows
    public TimeZoneInfo FindTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen"); // IANA ID (Linux/macOS)
        }
        catch (TimeZoneNotFoundException) { } // Ignore and try Windows ID
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); // Windows ID
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogCritical( // Using the injected logger
                ex,
                "Could not find Copenhagen timezone using either IANA ('Europe/Copenhagen') or Windows ('Central European Standard Time') ID."
            ); // Log a critical error if the Copenhagen timezone is not found using either IANA or Windows ID.
            throw; // Re-throw if neither is found, as this is critical for operation.
        }
    }
}
