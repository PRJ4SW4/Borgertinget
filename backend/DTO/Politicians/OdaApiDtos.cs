using System.Text.Json.Serialization;

namespace backend.DTO.FT
{
    public class UpdateAktor
    {
        public DateTime? slutdato { get; set; } //Kun for grupper, ministertitler- og -områder
        public DateTime? startdato { get; set; }
        public string? biografi { get; set; }
    }

    public class CreateAktor
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public string? navn { get; set; }

        public string? fornavn { get; set; }
        public string? efternavn { get; set; }
        public string? biografi { get; set; }
        public DateTime? startdato { get; set; }
        public DateTime? slutdato { get; set; }
    }

    // Generic wrapper for OData paged responses, easy extract nextLink
    public class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new List<T>();

        [JsonPropertyName("odata.nextLink")]
        public string? NextLink { get; set; }
    }

    public class AktorDetailDto
    {
        // send whole parsed aktor
        public int Id { get; set; }
        public string? fornavn { get; set; }
        public string? efternavn { get; set; }
        public string? navn { get; set; }
        public DateTime opdateringsdato { get; set; }

        // Parsed Biography Data fields
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
        public string? Ministertitel { get; set; }

        // Collections (assuming these are stored as strings or lists in your Aktor model) bliver serialized fra list<string> til string i db, og deserialized når vi henter fra db
        // Kan ses i Data/DataContext.cs under onModelCreate
        public List<string>? Ministers { get; set; }
        public List<string>? Spokesmen { get; set; }
        public List<string>? ParliamentaryPositionsOfTrust { get; set; }
        public List<string>? Constituencies { get; set; }
        public List<string>? Nominations { get; set; }
        public List<string>? Educations { get; set; }
        public List<string>? Occupations { get; set; }
        public List<string>? PublicationTitles { get; set; }

        //Helper method for mapping
        public static AktorDetailDto FromAktor(Models.Aktor aktor)
        {
            return new AktorDetailDto
            {
                Id = aktor.Id,
                fornavn = aktor.fornavn,
                efternavn = aktor.efternavn,
                navn = aktor.navn,
                opdateringsdato = aktor.opdateringsdato,
                Party = aktor.Party,
                PartyShortname = aktor.PartyShortname,
                Sex = aktor.Sex,
                Born = aktor.Born,
                EducationStatistic = aktor.EducationStatistic,
                PictureMiRes = aktor.PictureMiRes,
                FunctionFormattedTitle = aktor.FunctionFormattedTitle,
                FunctionStartDate = aktor.FunctionStartDate,
                PositionsOfTrust = aktor.PositionsOfTrust,
                Email = aktor.Email,
                Ministertitel = aktor.MinisterTitel,
                Ministers = aktor.Ministers,
                Spokesmen = aktor.Spokesmen,
                ParliamentaryPositionsOfTrust = aktor.ParliamentaryPositionsOfTrust,
                Constituencies = aktor.Constituencies,
                Nominations = aktor.Nominations,
                Educations = aktor.Educations,
                Occupations = aktor.Occupations,
                PublicationTitles = aktor.PublicationTitles,
            };
        }
    }

    // DTO for fetching Ministerial Titles (Aktør with typeid=2)
    public class MinisterialTitleDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("gruppenavnkort")]
        public string? GruppenavnKort { get; set; } // Short name of the title
    }

    // DTO for fetching Minister Relationships (AktørAktør with rolleid=8)
    public class MinisterRelationshipDto
    {
        [JsonPropertyName("fraaktørid")]
        public int FraAktorId { get; set; } // The Person's ID

        [JsonPropertyName("tilaktørid")]
        public int TilAktorId { get; set; } // The Title's ID (Aktør ID with typeid=2)
    }
}
