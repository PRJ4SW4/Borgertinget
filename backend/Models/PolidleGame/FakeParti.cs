namespace backend.Models;

using System.Collections.Generic; // Nødvendig for ICollection
using System.ComponentModel.DataAnnotations;

public class FakeParti
{
    [Key] // Primary Key
    public int PartiId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PartiNavn { get; set; } = string.Empty; // Initialiser

    //* Realation
    public ICollection<FakePolitiker> FakePolitikers { get; set; } = new List<FakePolitiker>();
}
