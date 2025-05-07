namespace backend.DTO;
public class PoliticianDetailsDto // Overvej at omdøbe til PoliticianDetailsDto?
{
    public int Id { get; set; }
    public string navn { get; set; } = string.Empty;
    // public Array[] Portræt { get; set; } // Sandsynligvis ikke nødvendig her
    public string? Sex { get; set; }
    public string? Party { get; set; } // Navn fra FakeParti
    public int Age { get; set; } // Tilføj denne (beregnet alder)
    public string? Region { get; set; }
    public List<string>? Educations { get; set; }
}