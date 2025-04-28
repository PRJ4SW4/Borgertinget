// DTO for the list/hierarchy view (doesn't need full content)
public class PageSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentPageId { get; set; }
    public int DisplayOrder { get; set; }
    public bool HasChildren { get; set; }
}

// DTO for the detailed page view (includes content)
public class PageDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // The Markdown content
    public int? ParentPageId { get; set; }

    // This is for previous and next buttons to work
    public int? PreviousSiblingId { get; set; }
    public int? NextSiblingId { get; set; }

    public List<QuestionDto> AssociatedQuestions { get; set; } = new List<QuestionDto>();
}

public class AnswerOptionDto
{
    public int Id { get; set; } // AnswerOptionId
    public string OptionText { get; set; } = string.Empty;
   
}

public class QuestionDto
{
    public int Id { get; set; } 
    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerOptionDto> Options { get; set; } = new List<AnswerOptionDto>();
}
