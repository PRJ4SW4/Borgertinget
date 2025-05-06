// /backend/DTO/Flashcards/FlashcardDTO.cs
namespace backend.DTO.Flashcards;

public class FlashcardDTO
{
    public int FlashcardId { get; set; }
    public string FrontContentType { get; set; } = "Text";
    public string? FrontText { get; set; }
    public string? FrontImagePath { get; set; } // Relative path

    public string BackContentType { get; set; } = "Text";
    public string? BackText { get; set; }
    public string? BackImagePath { get; set; } // Relative path
}

// NOTE:
// Images are stored in wwwroot/uploads/flashcards as pngs
// The relative path should be from uploads perspective so:
//    ex.      /uploads/flashcards/larsl.png
