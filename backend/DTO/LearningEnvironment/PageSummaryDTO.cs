// /backend/DTO/LearningEnvironment/PageSummaryDTO.cs
namespace backend.DTO.LearningEnvironment;

// DTO for the list/hierarchy view (doesn't need full content)
public class PageSummaryDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentPageId { get; set; }
    public int DisplayOrder { get; set; }
    public bool HasChildren { get; set; }
}
