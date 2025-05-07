// backend/Models/Politicians/Aktor.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Aktor
    {
        #region main info
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // If IDs come from an external source
        public int Id { get; set; } 
        public string? biografi {get; set; } = string.Empty; 
        public string? fornavn { get; set; } = string.Empty; 
        public string? efternavn { get; set; } = string.Empty; 
        public int? typeid { get; set; } 
        public string? gruppeNavnKort {get; set;} = string.Empty; 
        public string? navn {get; set;} = string.Empty; 
        public DateTime opdateringsdato {get; set;} 
        public int periodeid {get; set;} 
        public DateTime? slutdato {get; set;} 
        public DateTime? startdato {get; set;} 
        #endregion

        #region Parsed Biography Data
        public string? Party { get; set; } // String name of the party from bio
        public string? PartyShortname { get; set; } // Short string name from bio
        public string? Sex { get; set; } 
        public string? Born { get; set; } // String representation of birth date
        public string? EducationStatistic { get; set; }
        
        // This is the primary way to store the image now
        public byte[]? Portraet { get; set; } = Array.Empty<byte>(); 
        // PictureMiRes (string URL) can be removed if Portraet is always populated by downloading from this URL during import
        // public string? PictureMiRes { get; set; } 


        public string? FunctionFormattedTitle { get; set; }
        public string? FunctionStartDate { get; set; }
        public string? PositionsOfTrust { get; set; }
        public string? Email { get; set; }
        public string? MinisterTitel {get; set;}
        #endregion
        
        #region json collections
        public List<string>? Ministers { get; set; }
        public List<string>? Spokesmen { get; set; }
        public List<string>? ParliamentaryPositionsOfTrust { get; set; }
        public List<string>? Constituencies { get; set; }
        public List<string>? Nominations { get; set; }
        public List<string>? Educations { get; set; }   
        public List<string>? Occupations { get; set; }
        public List<string>? PublicationTitles { get; set; }
        #endregion

        #region Relations
        // Foreign Key to Party table
        public int partyId { get; set; } // This should be the FK to the Party table's PK

        [ForeignKey("partyId")] 
        public virtual Party? MembersOfParty { get; set; } // Navigation property to the Party

        public virtual ICollection<PolidleGamemodeTracker> GameTrackings { get; set; } = new List<PolidleGamemodeTracker>();
        public virtual ICollection<PoliticianQuote> Quotes { get; set; } = new List<PoliticianQuote>();
        #endregion  
    }

    // This class is typically used for deserializing API responses, not as an EF Core entity.
    // public class AktorResponse 
    // {
    //     public List<Aktor> Value { get; set; } = new List<Aktor>();
    // }
}
