namespace backend.DTO
{
    public class DailyPoliticianDto
    {
        public int Id { get; set; } 
        public string PolitikerNavn { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public string? Køn { get; set; }
        public string? PartyShortname { get; set; }
        public int Age { get; set; }
        public string? Region { get; set; }
        public string? Uddannelse { get; set; }
    }
}
