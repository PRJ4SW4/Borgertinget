namespace backend.utils.TimeZone;

public interface ITimeZoneHelper
{
    // Helper to find the timezone reliably on different OS
    // This makes sure it will work on both Linux, MacOS and Windows
    TimeZoneInfo FindTimeZone();
}
