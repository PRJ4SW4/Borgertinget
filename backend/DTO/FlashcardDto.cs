public class FlashcardDto
{
    public int FlashcardId { get; set; }
    public string FrontContentType { get; set; } = "Text"; // Enum as string
    public string? FrontText { get; set; }
    public string? FrontImagePath { get; set; } // Relative path

    public string BackContentType { get; set; } = "Text";
    public string? BackText { get; set; }
    public string? BackImagePath { get; set; } // Relative path
}
