namespace backend.DTO
{
    /// DTO der indeholder de nødvendige detaljer om en politiker
    /// specifikt til brug i Polidle-spillet (sammenligning og ledetråde).
    public class DailyPolticianDto
    {
        /// Unikt ID for politikeren (fra Aktor.Id).
        /// Bruges til at identificere den specifikke politiker.
        public int Id { get; set; }

        /// Politikerens fulde navn (fra Aktor.navn).
        /// Bruges som ledetråd/identifikation.
        public string PolitikerNavn { get; set; } = string.Empty;

        /// URL til politikerens portræt (fra Aktor.PictureMiRes).
        /// Bruges som ledetråd/identifikation.
        public string? PictureUrl { get; set; }

        /// Politikerens køn (fra Aktor.Sex).
        /// Bruges som ledetråd og til sammenligning.
        public string? Køn { get; set; }

        /// Politikerens parti (fulde navn, fra Aktor.Party).
        /// Bruges som ledetråd og til sammenligning.
        public string? Parti { get; set; }

        /// Politikerens beregnede alder (baseret på Aktor.Born).
        /// Bruges som ledetråd og til sammenligning (højere/lavere/lig).
        public int Age { get; set; } // Beregnes under mapping

        /// Politikerens primære region/valgkreds (Simplificeret fra Aktor.Constituencies).
        /// Bruges som ledetråd og til sammenligning.
        /// Kræver konsistent mapping-logik (f.eks. tag første element).
        public string? Region { get; set; } // Simplificeret under mapping

        /// Politikerens primære uddannelse/uddannelsesniveau (Simplificeret fra Aktor.Educations / Aktor.EducationStatistic).
        /// Bruges som ledetråd og til sammenligning.
        /// Kræver konsistent mapping-logik.
        public string? Uddannelse { get; set; } // Simplificeret under mapping
    }
}