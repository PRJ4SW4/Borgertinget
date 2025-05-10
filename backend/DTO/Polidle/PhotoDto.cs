namespace backend.DTO;

public class PhotoDto
{
    // Send som Base64 streng i JSON
    public string PortraitBase64 { get; set; } = string.Empty;
    // Alternativt: Send URL hvis billeder gemmes eksternt
    // public string PhotoUrl { get; set; } = string.Empty;
}
