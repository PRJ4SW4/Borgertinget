using System.Collections.Generic;

namespace backend.DTO
{
    // Enum til at angive typen af feedback per felt
    public enum FeedbackType
    {
        Undefined, // Bør ikke ske
        Korrekt,   // Værdien er korrekt
        Forkert,   // Værdien er forkert (bruges til Navn, Parti, Region, Køn, Uddannelse)
        Højere,    // Den korrekte værdi er højere (bruges til Alder)
        Lavere     // Den korrekte værdi er lavere (bruges til Alder)
        // Overvej 'Delvis' for f.eks. region/parti hvis det giver mening
    }

    // Indeholder detaljer om den gættede politiker - sendes med tilbage for nem visning i frontend
     public class GuessedPoliticianDetailsDto
    {
        public int Id { get; set; }
        public string PolitikerNavn { get; set; } = string.Empty;
        public string PartiNavn { get; set; } = string.Empty; // Navn fra FakeParti
        public int Age { get; set; } // Tilføjet for at beregne alder
        public string Køn { get; set; } = string.Empty;
        public string Uddannelse { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        // public byte[]? Portræt { get; set; } // Send evt. portræt med? Overvej størrelse.
    }

    public class GuessResultDto
    {
        /// <summary>
        /// Var gættet på den korrekte politiker (samme ID)?
        /// </summary>
        public bool IsCorrectGuess { get; set; }

        /// <summary>
        /// Detaljeret feedback per attribut (feltnavn -> feedbacktype).
        /// </summary>
        public Dictionary<string, FeedbackType> Feedback { get; set; } = new Dictionary<string, FeedbackType>();

         /// <summary>
        /// Detaljer om den politiker, der blev gættet på.
        /// </summary>
        public GuessedPoliticianDetailsDto? GuessedPolitician { get; set; }

        // Evt. tilføj antal gæt tilbage, spillets status etc.
        // public int GuessesRemaining { get; set; }
    }
}