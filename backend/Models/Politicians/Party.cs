using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models{
    public class Party{
        [Key]
        public int partyId {get; set;}

        public string? partyName {get; set;} = string.Empty;
        public string? partyShortName {get; set;} = string.Empty;

        public string? partyProgram {get; set;} = string.Empty;

        public string? poilitics {get; set;} = string.Empty ;

        public string? history {get; set;} = string.Empty;

        public List<string>? stats {get; set;}

        public int? chairmanId {get; set;}

        public int? viceChairmanId {get; set;}

        public int? secretaryId {get; set;}
        public int? spokesmanId {get; set;}



        [ForeignKey("chairmanId")]
        public virtual Aktor? chairman {get; set;} 
        [ForeignKey("viceChairmanId")]
        public virtual Aktor? viceChairman {get; set;}
        [ForeignKey("secretaryId")]
        public virtual Aktor? secretary {get; set;}
        [ForeignKey("spokesmanId")]
        public virtual Aktor? spokesman {get; set;}

        public List<int>? memberIds {get; set;}
    }
}