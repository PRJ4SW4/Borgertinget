namespace backend.DTOs
{
    public class PollOptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int Votes { get; set; }
    }
}
