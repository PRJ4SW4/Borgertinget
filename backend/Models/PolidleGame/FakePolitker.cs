using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Nødvendig for [ForeignKey]
using System.Collections.Generic; // Nødvendig for ICollection
using System;

namespace backend.Models;

public class FakePolitiker
{
    [Key] // Primary Key
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string PolitikerNavn { get; set; } = string.Empty; // Initialiser

    //! Ændret til DateOfBirth
    [Required]
    [Column("date_of_birth")]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [RegularExpression("^(Mand|Kvinde)$", ErrorMessage = "Køn must be either 'Mand' or 'Kvinde'")]
    public string Køn { get; set; } = string.Empty; // Initialiser

    [Required]
    [MaxLength(100)]
    public string Uddannelse { get; set; } = string.Empty; // Initialiser

    [Required]
    [MaxLength(150)]
    public string Region { get; set; } = string.Empty; // Initialiser

    [Required]
    public byte[] Portræt { get; set; } = Array.Empty<byte>(); // Store image as byte array

    // --- Relation til FakeParti ---

    // 1. Foreign Key Property (DENNE MANGLEREDE)
    //    Denne int-property gemmer Id'et for det tilknyttede FakeParti.
    //    Navnet 'PartiId' skal matche strengen i [ForeignKey]-attributten nedenfor
    //    og det, der bruges i DataContext's HasForeignKey-metode.
    public int PartiId { get; set; }

    // 2. Navigation Property til FakeParti (En-til-en fra Politikerens perspektiv)
    //    Giver adgang til det relaterede FakeParti-objekt.
    [ForeignKey("PartiId")] // Fortæller EF Core, at 'PartiId' property'en ovenfor er FK for denne navigation prop
    public FakeParti? FakeParti { get; set; } // '?' gør den valgfri (nullable)

    // --- Relation til PolidleGamemodeTracker (En-til-mange fra Politikerens perspektiv) ---
    // En FakePolitiker kan have mange Game Tracking entries
    public virtual ICollection<PolidleGamemodeTracker> GameTrackings { get; set; } = new List<PolidleGamemodeTracker>();

    // --- Relation til PoliticianQuote (en politiker har en række citater som bruges i spillet)
    public virtual ICollection<PoliticianQuote> Quotes { get; set; } = new List<PoliticianQuote>();
}