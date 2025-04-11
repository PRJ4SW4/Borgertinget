namespace backend.DTO; // Eller dit DTO namespace
public class DailyPoliticianDto
{
    public int Id { get; set; }
    public string PolitikerNavn { get; set; } = string.Empty;
    public Array[] Portræt { get; set; }
    public string? Køn { get; set; }    
    public string? Parti { get; set; } // Tilføj Party til FakePolitiker modellen
    public int? Alder { get; set; } //* Alder skal beregnes hvis du har DateOfBirth
    public string? Region { get; set; } 
    public string? Uddannelse { get; set; }

    // Tilføj flere felter/hints efter behov
}