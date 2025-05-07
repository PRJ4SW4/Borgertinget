using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models{
public class Party{
#region Attributes
    [Key]
    public int partyId {get; set;} //! ændret fra  PartiId i PolidleGame

    public string? partyName {get; set;} = string.Empty; //! ændret fra  PartiNavn i PolidleGame
    public string? partyShortName {get; set;} = string.Empty;

    public string? partyProgram {get; set;} = string.Empty;

    public string? politics {get; set;} = string.Empty ;

    public string? history {get; set;} = string.Empty;

    public List<string>? stats {get; set;}
#endregion

#region Relations
    public List<int>? memberIds {get; set;}

    //* Relation
    //TODO: Add a list of the politicians that are a member of a party
    public ICollection<Aktor> Politicians { get; set; } = new List<Aktor>();
#endregion
}
}