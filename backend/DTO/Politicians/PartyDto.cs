using System.ComponentModel.DataAnnotations;

namespace backend.DTO.FT;

public class UpdatePartyDto
{
    [Required]
    public int partyId { get; set; }

    public string? partyProgram { get; set; } = string.Empty;

    public string? politics { get; set; } = string.Empty;
    public string? history { get; set; } = string.Empty;
}

public class PartyDetailsDto
{
    public int partyId { get; set; }
    public string? partyName { get; set; }
    public string? partyShortName { get; set; }
    public string? partyProgram { get; set; }
    public string? politics { get; set; }
    public string? history { get; set; }
    public List<string>? stats { get; set; }
    public int? chairmanId { get; set; }
    public int? viceChairmanId { get; set; }
    public int? secretaryId { get; set; }
    public int? spokesmanId { get; set; }
    public List<int>? memberIds { get; set; }
}
