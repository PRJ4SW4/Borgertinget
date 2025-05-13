using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;


namespace backend.DTO.FT;

public class PartyDto {

    [Required]
    public int partyId {get; set;}

    public string? partyProgram {get; set;} = string.Empty;

    public string? politics {get; set;} = string.Empty;
    public string? history {get; set; } = string.Empty;




}