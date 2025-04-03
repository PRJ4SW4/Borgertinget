using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PagesController : ControllerBase
{
    private readonly DataContext _context;

    public PagesController(DataContext context)
    {
        _context = context;
    }

    // GET: api/pages - Get structure for navigation
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PageSummaryDto>>> GetPagesStructure()
    {
        // Fetch all pages, ordered for consistency
        var pages = await _context
            .Pages.OrderBy(p => p.ParentPageId) // Group parents together
            .ThenBy(p => p.DisplayOrder) // Order within siblings
            .ToListAsync();

        // Project to DTOs
        var pageSummaries = pages
            .Select(p => new PageSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                ParentPageId = p.ParentPageId,
                DisplayOrder = p.DisplayOrder,
                // Check if any page lists this page as its parent
                HasChildren = pages.Any(child => child.ParentPageId == p.Id),
            })
            .ToList();

        return Ok(pageSummaries);
    }

    // GET: api/pages/{id} - Get content for a specific page
    [HttpGet("{id}")]
    public async Task<ActionResult<PageDetailDto>> GetPage(int id)
    {
        var page = await _context.Pages.FindAsync(id);
        if (page == null)
        {
            return NotFound(); // Page itself not found
        }

        // --- Fetch ALL associated questions for this page ---
        var questions = await _context
            .Questions.Where(q => q.PageId == id) // Filter by PageId
            .Include(q => q.AnswerOptions.OrderBy(opt => opt.DisplayOrder))
            // Optional: Order the questions themselves if needed
            // .OrderBy(q => q.SomeQuestionOrderField)
            .ToListAsync(); // Get a list

        // Map the list of Question entities to a list of QuestionDto
        var questionDtos = questions
            .Select(question => new QuestionDto
            {
                Id = question.QuestionId,
                QuestionText = question.QuestionText,
                Options = question
                    .AnswerOptions.Select(opt => new AnswerOptionDto
                    {
                        Id = opt.AnswerOptionId,
                        OptionText = opt.OptionText,
                    })
                    .ToList(),
            })
            .ToList(); // Ensure this results in List<QuestionDto>
        // --- End fetching questions ---

        // --- Calculate the Previous/Next IDs using the new helper ---
        // Get the full ordered list of page IDs within the current page's section
        var sectionOrder = await GetSectionPageOrderEfficient(id, _context);

        int? previousPageId = null; // Use the DTO's existing property names
        int? nextPageId = null; // Use the DTO's existing property names

        // Find the position (index) of the current page within the ordered list
        int currentIndex = sectionOrder.IndexOf(id);

        if (currentIndex == -1)
        {
            // Should not happen if page exists, but handle defensively
            // Maybe log a warning here
        }
        else
        {
            // Assign previous ID if current page is not the first in the list
            if (currentIndex > 0)
            {
                previousPageId = sectionOrder[currentIndex - 1];
            }
            // Assign next ID if current page is not the last in the list
            if (currentIndex < sectionOrder.Count - 1)
            {
                nextPageId = sectionOrder[currentIndex + 1];
            }
        }
        // --- End calculation ---

        // Populate the DTO to be sent to the frontend
        var pageDetail = new PageDetailDto
        {
            Id = page.Id,
            Title = page.Title,
            Content = page.Content,
            ParentPageId = page.ParentPageId,
            // Assign the calculated sequence IDs (reusing the existing DTO fields)
            PreviousSiblingId = previousPageId, // Corresponds to previousPageId variable
            NextSiblingId = nextPageId, // Corresponds to nextPageId variable
            AssociatedQuestions = questionDtos, // Assign the LIST of questions
        };

        return Ok(pageDetail);
    }

    private async Task<List<int>> GetSectionPageOrderEfficient(
        int currentPageId,
        DataContext context
    )
    {
        // --- Step 1: Find the root of the current page's section ---
        Page? currentPage = await context.Pages.FindAsync(currentPageId);
        if (currentPage == null)
            return new List<int>(); // Return empty list if page not found

        Page sectionRoot = currentPage;
        int? parentId = sectionRoot.ParentPageId;

        // Traverse upwards to find the top-level page (where ParentPageId is null)
        // This loop might require loading parent entities if not already included/tracked
        while (parentId != null)
        {
            Page? parentPage = await context.Pages.FindAsync(parentId);
            if (parentPage == null)
                break; // Stop if parent is missing (data integrity issue)
            sectionRoot = parentPage;
            parentId = sectionRoot.ParentPageId;
        }

        // --- Step 2: Use Raw SQL with Recursive CTE for ordered descendants ---
        // IMPORTANT: Replace "Pages" with your actual table name if it's different.
        // PostgreSQL table names might be case-sensitive, especially if created with quotes.
        // Check your database schema or DbContext configuration (e.g., [Table("Pages")] attribute or fluent API .ToTable("Pages")).
        string sql =
            @"
        WITH RECURSIVE SectionPages AS (
            -- Anchor member: Define aliases as lowercase for clarity, though PG folds unquoted ones anyway
            SELECT
                ""Pages"".""Id"",           -- Inherited column, keep PascalCase quoted
                ""Pages"".""ParentPageId"", -- Inherited column, keep PascalCase quoted
                ""Pages"".""DisplayOrder"", -- Inherited column, keep PascalCase quoted
                1 AS level,                 -- Use lowercase alias (becomes 'level')
                LPAD(""Pages"".""DisplayOrder""::TEXT, 5, '0') AS sortpath -- Use lowercase alias (becomes 'sortpath')
            FROM ""Pages"" -- Your table name
            WHERE ""Pages"".""Id"" = {0}

            UNION ALL

            -- Recursive member: Use lowercase for aliased CTE columns, quoted PascalCase for others
            SELECT
                p.""Id"",           -- From table p, keep PascalCase quoted
                p.""ParentPageId"", -- From table p, keep PascalCase quoted
                p.""DisplayOrder"", -- From table p, keep PascalCase quoted
                sp.level + 1,       -- Reference lowercase 'level' from CTE sp (unquoted)
                sp.sortpath || '.' || LPAD(p.""DisplayOrder""::TEXT, 5, '0') -- Reference lowercase 'sortpath' from CTE sp (unquoted)
            FROM ""Pages"" p
            -- JOIN using quoted PascalCase 'Id' from CTE sp and table p
            INNER JOIN SectionPages sp ON p.""ParentPageId"" = sp.""Id""

        )
        -- Final selection: Use quoted PascalCase for inherited 'Id'
        SELECT ""Id""
        FROM SectionPages
        -- Order by lowercase aliased 'sortpath'
        ORDER BY sortpath;
    ";

        // Execute the raw SQL query
        var orderedIds = await context
            .Database.SqlQueryRaw<int>(sql, sectionRoot.Id) // Pass sectionRoot.Id as parameter {0}
            .ToListAsync();

        return orderedIds;
    }
    // --- Add POST, PUT, DELETE endpoints later ---
    // Add [Authorize(Roles = "Admin")] attribute later
}
