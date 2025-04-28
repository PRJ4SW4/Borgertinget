namespace backend.DTO
{
    public class PoliticianSummaryDto
    {
        public int Id { get; set; }
        public string PolitikerNavn { get; set; } = string.Empty;
        public byte[] Portraet { get; set; } = Array.Empty<byte>(); // Portræt som byte array
    }
}