namespace backend.DTO; // Eller dit DTO namespace
public class DailyPoliticianDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Parti { get; set; } // Tilføj Party til FakePolitiker modellen
    public string? Region { get; set; }
    public string? Køn { get; set; } 
    public int? Alder { get; set; } //* Alder skal beregnes hvis du har DateOfBirth

    // Tilføj flere felter/hints efter behov
}