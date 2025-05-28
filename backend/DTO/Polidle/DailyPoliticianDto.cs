namespace backend.DTO
{
    public class DailyPoliticianDto
    {
        public int Id { get; set; } // identificere den specifikke politiker
        public string PolitikerNavn { get; set; } = string.Empty; // ledetråd
        public string? PictureUrl { get; set; } // ledetråd
        public string? Køn { get; set; } // ledetråd og til sammenligning
        public string? PartyShortname { get; set; } // ledetråd og til sammenlignin
        public int Age { get; set; } // Beregnes under mapping, ledetråd og til sammenligning (højere/lavere)
        public string? Region { get; set; }
        public string? Uddannelse { get; set; }
    }
}
