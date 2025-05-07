using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace backend.Services
{
    public static class DataParsingHelpers
    {
        /// <summary>
        /// Parses a string representation of a birth date into a DateTime object.
        /// It attempts to parse various common date formats.
        /// The 'Born' string from ODA often appears as "yyyy-MM-ddT00:00:00".
        /// </summary>
        /// <param name="bornString">The string containing the birth date.</param>
        /// <param name="logger">Optional logger for recording parsing issues.</param>
        /// <returns>A DateTime object if parsing is successful; otherwise, null.</returns>
        public static DateTime? ParseBornStringToDateTime(string? bornString, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(bornString))
            {
                return null;
            }

            // Common formats to try. The ODA API often returns dates in ISO 8601 format.
            string[] formats = {
                "yyyy-MM-ddTHH:mm:ss", // Standard ISO 8601 format often seen in XML/JSON APIs
                "yyyy-MM-dd",
                "dd-MM-yyyy",
                "MM/dd/yyyy",
                "yyyy/MM/dd",
                "dd.MM.yyyy",
                "yyyy" // For cases where only the year might be provided
            };

            DateTime parsedDate;
            if (DateTime.TryParseExact(bornString, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsedDate))
            {
                // Ensure it's Kind.Utc if parsed as universal, or specify if not already.
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }
            else if (DateTime.TryParse(bornString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsedDate))
            {
                // Fallback to general TryParse if exact formats fail
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }
            else
            {
                logger?.LogWarning("Could not parse 'Born' string '{BornString}' into a DateTime.", bornString);
                return null;
            }
        }

        /// <summary>
        /// Extracts the first education from a list of education strings.
        /// </summary>
        /// <param name="educations">A list of strings representing educations.</param>
        /// <returns>The first education string if the list is not null or empty; otherwise, null.</returns>
        public static string? GetFirstEducation(List<string>? educations)
        {
            if (educations != null && educations.Any())
            {
                return educations[0]?.Trim(); // Return the first education, trimmed.
            }
            return null; // Return null if the list is null, empty, or the first item is null/whitespace.
        }
    }
}