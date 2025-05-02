// PoliticianQuote.cs
namespace backend.DTOs;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class EditQuoteDTO
{
    public int QuoteId { get; set; }
    public string QuoteText { get; set; } = string.Empty;
}
