// /backend/Controllers/LearningEnvironment/PagesController.cs
namespace backend.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Specifies that this class is an API controller, handling requests related to pages.
[Route("api/[controller]")]
[ApiController]
public class PagesController : ControllerBase
{
    // A private readonly field to hold the DataContext instance, enabling database interactions.
    private readonly DataContext _context;

    // Constructor for the PagesController, injecting the DataContext via dependency injection.
    public PagesController(DataContext context)
    {
        // Assigns the injected DataContext instance to the private field for use within the controller.
        _context = context;
    }

    // Defines an HTTP GET endpoint to retrieve the structure of pages, typically for navigation purposes.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PageSummaryDto>>> GetPagesStructure()
    {
        // Asynchronously retrieves all Page entities from the database, applying ordering for consistent structure.
        var pages = await _context
            .Pages
            // Orders pages primarily by their ParentPageId, grouping pages within the same section together.
            .OrderBy(p => p.ParentPageId)
            // Orders pages secondarily by their DisplayOrder, ensuring correct sequence within each section.
            .ThenBy(p => p.DisplayOrder)
            // Executes the query and returns the results as a List.
            .ToListAsync();

        // Projects the retrieved Page entities into PageSummaryDto objects, shaping the data for the response.
        var pageSummaries = pages
            .Select(p => new PageSummaryDto
            {
                // Maps the Id property from the Page entity to the DTO.
                Id = p.Id,
                // Maps the Title property from the Page entity to the DTO.
                Title = p.Title,
                // Maps the ParentPageId property from the Page entity to the DTO.
                ParentPageId = p.ParentPageId,
                // Maps the DisplayOrder property from the Page entity to the DTO.
                DisplayOrder = p.DisplayOrder,
                // Determines if the current page has children by checking if any other page's ParentPageId matches the current page's Id.
                HasChildren = pages.Any(child => child.ParentPageId == p.Id),
            })
            // Converts the resulting IEnumerable<PageSummaryDto> to a List.
            .ToList();

        // Returns an HTTP 200 OK response containing the list of page summaries.
        return Ok(pageSummaries);
    }

    // Defines an HTTP GET endpoint to retrieve detailed information for a specific page, identified by its ID.
    [HttpGet("{id}")]
    public async Task<ActionResult<PageDetailDto>> GetPage(int id)
    {
        // Asynchronously attempts to find a Page entity with the specified ID in the database.
        var page = await _context.Pages.FindAsync(id);

        // Checks if the page was found in the database.
        if (page == null)
        {
            // Returns an HTTP 404 Not Found response if no page with the given ID exists.
            return NotFound();
        }

        // Asynchronously retrieves all Question entities associated with the current page, including their AnswerOptions.
        var questions = await _context
            .Questions
            // Filters questions to include only those belonging to the current page.
            .Where(q => q.PageId == id)
            // Eagerly loads the related AnswerOptions for each question, ordered by their DisplayOrder.
            .Include(q => q.AnswerOptions.OrderBy(opt => opt.DisplayOrder))
            // Executes the query and returns the results as a List.
            .ToListAsync();

        // Projects the retrieved Question entities into QuestionDto objects, shaping the data for the response.
        var questionDtos = questions
            .Select(question => new QuestionDto
            {
                // Maps the QuestionId property from the Question entity to the DTO.
                Id = question.QuestionId,
                // Maps the QuestionText property from the Question entity to the DTO.
                QuestionText = question.QuestionText,
                // Projects the associated AnswerOptions into AnswerOptionDto objects.
                Options = question
                    .AnswerOptions.Select(opt => new AnswerOptionDto
                    {
                        // Maps the AnswerOptionId property from the AnswerOption entity to the DTO.
                        Id = opt.AnswerOptionId,
                        // Maps the OptionText property from the AnswerOption entity to the DTO.
                        OptionText = opt.OptionText,
                    })
                    // Converts the resulting IEnumerable<AnswerOptionDto> to a List.
                    .ToList(),
            })
            // Converts the resulting IEnumerable<QuestionDto> to a List.
            .ToList();

        // Asynchronously retrieves the ordered list of page IDs within the same section as the current page.
        var sectionOrder = await GetSectionPageOrderEfficient(id, _context);

        // Initializes variables to store the IDs of the previous and next pages in the sequence.
        int? previousPageId = null;
        int? nextPageId = null;

        // Finds the index of the current page's ID within the ordered list of page IDs in the section.
        int currentIndex = sectionOrder.IndexOf(id);

        // Checks if the current page ID was found in the ordered list.
        if (currentIndex != -1)
        {
            // If the current page is not the first page in the list, assign the ID of the preceding page to previousPageId.
            if (currentIndex > 0)
            {
                previousPageId = sectionOrder[currentIndex - 1];
            }
            // If the current page is not the last page in the list, assign the ID of the succeeding page to nextPageId.
            if (currentIndex < sectionOrder.Count - 1)
            {
                nextPageId = sectionOrder[currentIndex + 1];
            }
        }

        // Creates a new PageDetailDto object to encapsulate the detailed page information for the response.
        var pageDetail = new PageDetailDto
        {
            // Maps the Id property from the Page entity to the DTO.
            Id = page.Id,
            // Maps the Title property from the Page entity to the DTO.
            Title = page.Title,
            // Maps the Content property from the Page entity to the DTO.
            Content = page.Content,
            // Maps the ParentPageId property from the Page entity to the DTO.
            ParentPageId = page.ParentPageId,
            // Assigns the calculated ID of the previous page in the sequence.
            PreviousSiblingId = previousPageId,
            // Assigns the calculated ID of the next page in the sequence.
            NextSiblingId = nextPageId,
            // Assigns the list of associated questions (as DTOs) to the DTO.
            AssociatedQuestions = questionDtos,
        };

        // Returns an HTTP 200 OK response containing the detailed page information.
        return Ok(pageDetail);
    }

    // A private helper method to efficiently determine the display order of pages within a specific section.
    private async Task<List<int>> GetSectionPageOrderEfficient(
        int currentPageId,
        DataContext context
    )
    {
        // Attempts to find the current page entity based on the provided ID.
        Page? currentPage = await context.Pages.FindAsync(currentPageId);

        // If the current page doesn't exist, return an empty list immediately.
        if (currentPage == null)
            return new List<int>();

        // Initializes the section root to the current page.
        Page sectionRoot = currentPage;
        // Gets the parent ID of the current potential root.
        int? parentId = sectionRoot.ParentPageId;

        // Iteratively traverses up the page hierarchy until a page with no parent (null ParentPageId) is found.
        while (parentId != null)
        {
            // Finds the parent page entity.
            Page? parentPage = await context.Pages.FindAsync(parentId);

            // If a parent page in the hierarchy is missing, stop traversing.
            if (parentPage == null)
                break;

            // Updates the section root to the parent page found.
            sectionRoot = parentPage;

            // Updates the parent ID for the next iteration.
            parentId = sectionRoot.ParentPageId;
        }

        // Raw SQL query using a recursive CTE to efficiently retrieve all descendant pages of the section root, ordered by their display order.
        string sql =
            @"
        WITH RECURSIVE SectionPages AS (
            -- Anchor member: Select the root page of the section.
            SELECT
                ""Pages"".""Id"",
                ""Pages"".""ParentPageId"",
                ""Pages"".""DisplayOrder"",
                1 AS level,
                LPAD(""Pages"".""DisplayOrder""::TEXT, 5, '0') AS sortpath
            FROM ""Pages""
            WHERE ""Pages"".""Id"" = {0}

            UNION ALL

            -- Recursive member: Join child pages to their parents within the section.
            SELECT
                p.""Id"",
                p.""ParentPageId"",
                p.""DisplayOrder"",
                sp.level + 1,
                sp.sortpath || '.' || LPAD(p.""DisplayOrder""::TEXT, 5, '0')
            FROM ""Pages"" p
            INNER JOIN SectionPages sp ON p.""ParentPageId"" = sp.""Id""
        )
        -- Final selection: Select the Id of each page in the section, ordered by the constructed sort path.
        SELECT ""Id""
        FROM SectionPages
        ORDER BY sortpath;
    ";

        // Executes the raw SQL query and retrieves the ordered list of page IDs.
        var orderedIds = await context.Database.SqlQueryRaw<int>(sql, sectionRoot.Id).ToListAsync();

        // Returns the ordered list of page IDs within the section.
        return orderedIds;
    }

    // TODO: Add POST, PUT, DELETE endpoints later
    // This should be done by Kevin and Olivia, as they are responsible for administration and it depends on their implementation
}
