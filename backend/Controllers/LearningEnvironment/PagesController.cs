// /backend/Controllers/LearningEnvironment/PagesController.cs
namespace backend.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // Defines an HTTP GET endpoint to retrieve the structure of pages, for navigation purposes.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PageSummaryDTO>>> GetPagesStructure()
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
            .Select(p => new PageSummaryDTO
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
    public async Task<ActionResult<PageDetailDTO>> GetPage(int id)
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
            .Select(question => new QuestionDTO
            {
                // Maps the QuestionId property from the Question entity to the DTO.
                Id = question.QuestionId,
                // Maps the QuestionText property from the Question entity to the DTO.
                QuestionText = question.QuestionText,
                // Projects the associated AnswerOptions into AnswerOptionDto objects.
                Options = question
                    .AnswerOptions.Select(opt => new AnswerOptionDTO
                    {
                        // Maps the AnswerOptionId property from the AnswerOption entity to the DTO.
                        Id = opt.AnswerOptionId,
                        // Maps the OptionText property from the AnswerOption entity to the DTO.
                        OptionText = opt.OptionText,
                        // Maps the IsCorrect property from the AnswerOption entity to the DTO.
                        IsCorrect = opt.IsCorrect,
                        // Maps the DisplayOrder property from the AnswerOption entity to the DTO.
                        DisplayOrder = opt.DisplayOrder,
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
        var pageDetail = new PageDetailDTO
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

    // POST: api/pages
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PageDetailDTO>> CreatePage(PageCreateRequestDTO createRequest)
    {
        var newPage = new Page
        {
            Title = createRequest.Title,
            Content = createRequest.Content,
            ParentPageId = createRequest.ParentPageId,
            DisplayOrder = createRequest.DisplayOrder,
            AssociatedQuestions = createRequest
                .AssociatedQuestions.Select(qDto => new Question
                {
                    QuestionText = qDto.QuestionText,
                    AnswerOptions = qDto
                        .Options.Select(optDto => new AnswerOption
                        {
                            OptionText = optDto.OptionText,
                            IsCorrect = optDto.IsCorrect,
                            DisplayOrder = optDto.DisplayOrder,
                        })
                        .ToList(),
                })
                .ToList(),
        };

        _context.Pages.Add(newPage);
        await _context.SaveChangesAsync();

        // Return the created page details, similar to GetPage(id)
        // This requires mapping the newPage entity back to PageDetailDTO
        // For simplicity, we'll call GetPage, but ideally, map directly or return a simpler confirmation.
        return CreatedAtAction(
            nameof(GetPage),
            new { id = newPage.Id },
            MapPageToPageDetailDTO(newPage, null, null)
        );
    }

    // PUT: api/pages/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePage(int id, PageUpdateRequestDTO updateRequest)
    {
        if (id != updateRequest.Id)
        {
            return BadRequest("Page ID mismatch.");
        }

        var existingPage = await _context
            .Pages.Include(p => p.AssociatedQuestions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPage == null)
        {
            return NotFound();
        }

        // Update scalar properties
        existingPage.Title = updateRequest.Title;
        existingPage.Content = updateRequest.Content;
        existingPage.ParentPageId = updateRequest.ParentPageId;
        existingPage.DisplayOrder = updateRequest.DisplayOrder;

        // Manage AssociatedQuestions
        // Remove questions not in the update request or deleted by user
        var questionsToRemove = existingPage
            .AssociatedQuestions.Where(eq =>
                !updateRequest.AssociatedQuestions.Any(uqDto =>
                    uqDto.Id == eq.QuestionId && uqDto.Id != 0
                )
            )
            .ToList();
        _context.Questions.RemoveRange(questionsToRemove);

        foreach (var qDto in updateRequest.AssociatedQuestions)
        {
            Question? existingQuestion = null;
            if (qDto.Id != 0) // If Id is provided, try to find existing question
            {
                existingQuestion = existingPage.AssociatedQuestions.FirstOrDefault(q =>
                    q.QuestionId == qDto.Id
                );
            }

            if (existingQuestion != null) // Update existing question
            {
                existingQuestion.QuestionText = qDto.QuestionText;

                // Manage AnswerOptions for the existing question
                var optionsToRemove = existingQuestion
                    .AnswerOptions.Where(eo =>
                        !qDto.Options.Any(uoDto => uoDto.Id == eo.AnswerOptionId && uoDto.Id != 0)
                    )
                    .ToList();
                _context.AnswerOptions.RemoveRange(optionsToRemove);

                foreach (var optDto in qDto.Options)
                {
                    AnswerOption? existingOption = null;
                    if (optDto.Id != 0)
                    {
                        existingOption = existingQuestion.AnswerOptions.FirstOrDefault(o =>
                            o.AnswerOptionId == optDto.Id
                        );
                    }

                    if (existingOption != null) // Update existing option
                    {
                        existingOption.OptionText = optDto.OptionText;
                        existingOption.IsCorrect = optDto.IsCorrect;
                        existingOption.DisplayOrder = optDto.DisplayOrder;
                    }
                    else // Add new option
                    {
                        existingQuestion.AnswerOptions.Add(
                            new AnswerOption
                            {
                                OptionText = optDto.OptionText,
                                IsCorrect = optDto.IsCorrect,
                                DisplayOrder = optDto.DisplayOrder,
                                // QuestionId will be set by EF
                            }
                        );
                    }
                }
            }
            else // Add new question
            {
                var newQuestion = new Question
                {
                    QuestionText = qDto.QuestionText,
                    AnswerOptions = qDto
                        .Options.Select(optDto => new AnswerOption
                        {
                            OptionText = optDto.OptionText,
                            IsCorrect = optDto.IsCorrect,
                            DisplayOrder = optDto.DisplayOrder,
                        })
                        .ToList(),
                    // PageId will be set by EF
                };
                existingPage.AssociatedQuestions.Add(newQuestion);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PageExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/pages/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePage(int id)
    {
        var page = await _context.Pages.FindAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        _context.Pages.Remove(page);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PageExists(int id)
    {
        return _context.Pages.Any(e => e.Id == id);
    }

    // Helper to map Page to PageDetailDTO (can be expanded or moved to a service)
    private PageDetailDTO MapPageToPageDetailDTO(Page page, int? prevId, int? nextId)
    {
        return new PageDetailDTO
        {
            Id = page.Id,
            Title = page.Title,
            Content = page.Content,
            ParentPageId = page.ParentPageId,
            PreviousSiblingId = prevId, // These would typically be calculated as in GetPage
            NextSiblingId = nextId, // For CreatedAtAction, they might be null or require re-query
            AssociatedQuestions =
                page.AssociatedQuestions?.Select(q => new QuestionDTO
                    {
                        Id = q.QuestionId,
                        QuestionText = q.QuestionText,
                        Options =
                            q.AnswerOptions?.OrderBy(o => o.DisplayOrder)
                                .Select(opt => new AnswerOptionDTO
                                {
                                    Id = opt.AnswerOptionId,
                                    OptionText = opt.OptionText,
                                    IsCorrect = opt.IsCorrect,
                                    DisplayOrder = opt.DisplayOrder,
                                })
                                .ToList() ?? new List<AnswerOptionDTO>(),
                    })
                    .ToList() ?? new List<QuestionDTO>(),
        };
    }
}
