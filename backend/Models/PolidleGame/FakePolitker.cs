using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Nødvendig for ICollection

public class FakePolitiker
{
    [Key] // Primary Key
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string PolitikerNavn { get; set; } = string.Empty; // Initialiser
    [Required]
    [Range(18, 90, ErrorMessage = "Age must be between 18 and 90")]
    public int Alder { get; set; }
    [Required]
    [RegularExpression("^(Mand|Kvinde)$", ErrorMessage = "Køn must be either 'Mand' or 'Kvinde'")]
    public string Køn { get; set; } = string.Empty; // Initialiser
    [Required]
    [MaxLength(100)]
    public string Uddannelse { get; set; } = string.Empty; // Initialiser
    [Required]
    [MaxLength(150)]
    public string Region { get; set; } = string.Empty; // Initialiser

    // --- Korrekt Relation (En-til-mange) ---
    // En FakePolitiker kan have mange Game Tracking entries
    public virtual ICollection<PolidleGamemodeTracker> GameTrackings { get; set; } = new List<PolidleGamemodeTracker>();
}