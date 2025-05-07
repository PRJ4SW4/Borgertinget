// PoliticianQuote.cs
namespace backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PoliticianQuote
{
    [Key] // Primary Key
    public int QuoteId { get; set; }

    [Required]
    public string QuoteText { get; set; } = string.Empty;

    // Foreign key to Aktor
    public int PolitikerId { get; set; }

    [ForeignKey(nameof(PolitikerId))]
    public virtual Aktor Aktor { get; set; } = null!;
}