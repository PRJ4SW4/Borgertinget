namespace backend.DTO.LearningEnvironment;

// DTO for the detailed page view (includes content)
public class PageDetailDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // The Markdown content
    public int? ParentPageId { get; set; }

    // This is for previous and next buttons to work
    public int? PreviousSiblingId { get; set; }
    public int? NextSiblingId { get; set; }

    public List<QuestionDTO> AssociatedQuestions { get; set; } = new List<QuestionDTO>();
}
