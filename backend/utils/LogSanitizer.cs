// Fil: backend/Utils/LogSanitizer.cs (eller et passende namespace)
namespace backend.Utils // Tilpas namespace
{
    public static class LogSanitizer
    {
        public static string Sanitize(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "<empty_or_null>"; // Gør det tydeligt i loggen
            }
            // Erstat linjeskift (CR, LF) og tabs med noget ufarligt (f.eks. en underscore eller tom streng)
            // Dette forhindrer, at brugerinput kan skabe nye loglinjer eller forskyde formatering.
            return input.Replace("\n", "_").Replace("\r", "_").Replace("\t", "_");
        }
    }
}