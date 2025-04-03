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
}
