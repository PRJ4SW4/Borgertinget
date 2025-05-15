namespace backend.Enums;

// Enum til at angive Gamemode typen
public enum GamemodeTypes
{
    Klassisk = 0, // Default
    Citat = 1,
    Foto = 2,
}

// Enum til at angive typen af feedback per felt
public enum FeedbackType
{
    Undefined, // Bør ikke ske
    Korrekt, // Værdien er korrekt
    Forkert, // Værdien er forkert (bruges til Navn, Parti, Region, Køn, Uddannelse)
    Højere, // Den korrekte værdi er højere (bruges til Alder)
    Lavere, // Den korrekte værdi er lavere (bruges til Alder)
    // Overvej 'Delvis' for f.eks. region/parti hvis det giver mening
}
