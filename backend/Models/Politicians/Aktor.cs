using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Aktor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } // Unik id
        public string? biografi { get; set; } = string.Empty; // Biografi for personen eller gruppen
        public string? fornavn { get; set; } = string.Empty; //fornavn for personen
        public string? efternavn { get; set; } = string.Empty; //efternavn for personen
        public int? typeid { get; set; } //Type(Udvalg, Parlamentarisk forsamling, Anden gruppe, Folketingsgruppe, Kommission, Ministerområde, Ministertitel)
        public string? gruppeNavnKort { get; set; } = string.Empty; //Forkortelse - hvis aktøren er en gruppe (udvalg, kommission, folketingsgruppe)
        public string? navn { get; set; } = string.Empty; //Personens for- og efternavn - ellers lang form af navn
        public DateTime opdateringsdato { get; set; } // Dato for opdatering

        public int periodeid { get; set; } //Kun for udvalg, Folketinget og kommissioner og hverv. I disse tilfælde angiver det gruppens tilknytning til periodetypen 'samling'
        public DateTime? slutdato { get; set; } //Kun for grupper, ministertitler- og -områder
        public DateTime? startdato { get; set; } //kun for grupper, ministertitler- og -områder

        // --- Parsed Biography Data ---
        public string? Party { get; set; }
        public string? PartyShortname { get; set; }
        public string? Sex { get; set; }
        public string? Born { get; set; }
        public string? EducationStatistic { get; set; }
        public string? PictureMiRes { get; set; }
        public string? FunctionFormattedTitle { get; set; }
        public string? FunctionStartDate { get; set; }
        public string? PositionsOfTrust { get; set; }
        public string? Email { get; set; }

        public string? MinisterTitel { get; set; }

        // json collections
        public List<string>? Ministers { get; set; }
        public List<string>? Spokesmen { get; set; }
        public List<string>? ParliamentaryPositionsOfTrust { get; set; }
        public List<string>? Constituencies { get; set; }
        public List<string>? Nominations { get; set; }
        public List<string>? Educations { get; set; }
        public List<string>? Occupations { get; set; }
        public List<string>? PublicationTitles { get; set; }

        #region Realations
        //* Quotes
        public virtual ICollection<PoliticianQuote> Quotes { get; set; } =
            new List<PoliticianQuote>();

        //* GamemodeTracker
        public virtual ICollection<GamemodeTracker> GamemodeTrackings { get; set; } =
            new List<GamemodeTracker>();

        //* DailySelection
        public virtual ICollection<DailySelection> DailySelections { get; set; } =
            new List<DailySelection>();
        #endregion
    }

    public class AktorResponse
    {
        public List<Aktor> Value { get; set; } = new List<Aktor>();
    }
}
