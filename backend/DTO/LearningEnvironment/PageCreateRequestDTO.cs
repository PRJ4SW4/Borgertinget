namespace backend.DTO.LearningEnvironment;

using System.ComponentModel.DataAnnotations;

public class PageCreateRequestDTO
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int? ParentPageId { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public List<QuestionCreateOrUpdateDTO> AssociatedQuestions { get; set; } =
        new List<QuestionCreateOrUpdateDTO>();
}
