namespace backend.DTO;
public class PoliticianDetailsDto // Overvej at omdøbe til PoliticianDetailsDto?
{
    public int Id { get; set; }
    public string PolitikerNavn { get; set; } = string.Empty;
    // public Array[] Portræt { get; set; } // Sandsynligvis ikke nødvendig her
    public string? Køn { get; set; }
    public string? Parti { get; set; } // Navn fra FakeParti

    public int Age { get; set; } // Tilføj denne (beregnet alder)

    public string? Region { get; set; }
    public string? Uddannelse { get; set; }
}