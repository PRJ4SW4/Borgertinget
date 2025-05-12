using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using backend.Services.LearningEnvironmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PagesController : ControllerBase
{
    // A private readonly field to hold the ILearningPageService instance.
    private readonly ILearningPageService _pageService;

    // Constructor for the PagesController, injecting the ILearningPageService.
    public PagesController(ILearningPageService pageService)
    {
        _pageService = pageService;
    }

    // Defines an HTTP GET endpoint at "api/pages/structure" to retrieve the hierarchical structure of pages.
    [HttpGet("structure")]
    public async Task<ActionResult<IEnumerable<PageSummaryDTO>>> GetPagesStructure()
    {
        // Calls the service to get the page structure.
        var structure = await _pageService.GetPagesStructureAsync();
        // Returns an HTTP 200 OK response with the page structure.
        return Ok(structure);
    }

    // Defines an HTTP GET endpoint at "api/pages/{id}" to retrieve the details of a specific page.
    [HttpGet("{id}")]
    public async Task<ActionResult<PageDetailDTO>> GetPage(int id)
    {
        // Calls the service to get page details.
        var pageDetail = await _pageService.GetPageDetailAsync(id);

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

        // Returns an HTTP 200 OK response with the page details.
        return Ok(pageDetail);
    }

    // Defines an HTTP GET endpoint at "api/pages/{id}/sectionorder"
    // to retrieve the ordered list of page IDs within the same section as the given page.
    [HttpGet("{id}/sectionorder")]
    public async Task<ActionResult<List<int>>> GetSectionPageOrder(int id)
    {
        // Calls the service to get the section page order.
        var orderedPageIds = await _pageService.GetSectionPageOrderAsync(id);

        // If the list is empty (e.g., page not found or no pages in section),
        // it might still be a valid scenario, so we return Ok with the list.
        // The service handles logging if the page itself isn't found.
        return Ok(orderedPageIds);
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
