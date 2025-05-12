namespace backend.Services.LearningEnvironmentServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Service responsible for handling logic related to learning pages.
public class LearningPageService : ILearningPageService
{
    // A private readonly field to hold the DataContext instance, enabling database interactions.
    private readonly DataContext _context;

    // A private readonly field for logging.
    private readonly ILogger<LearningPageService> _logger;

    // Constructor for the LearningPageService, injecting the DataContext and ILogger.
    public LearningPageService(DataContext context, ILogger<LearningPageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Asynchronously retrieves the hierarchical structure of all learning pages.
    public async Task<IEnumerable<PageSummaryDTO>> GetPagesStructureAsync()
    {
        // Retrieves all pages from the database.
        var allPages = await _context.Pages.ToListAsync();

        // Maps the Page entities to PageSummaryDTOs.
        // For each page, it determines if it has child pages.
        return allPages
            .Select(p => new PageSummaryDTO
            {
                Id = p.Id,
                Title = p.Title,
                ParentPageId = p.ParentPageId,
                DisplayOrder = p.DisplayOrder,
                HasChildren = allPages.Any(child => child.ParentPageId == p.Id),
            })
            .OrderBy(p => p.ParentPageId ?? -1) // Ensures root pages come first
            .ThenBy(p => p.DisplayOrder)
            .ToList();
    }

    // Asynchronously retrieves the detailed content of a specific learning page.
    public async Task<PageDetailDTO?> GetPageDetailAsync(int pageId)
    {
        // Retrieves the specified page by its ID, including its related questions and their answer options.
        var page = await _context
            .Pages.Include(p => p.AssociatedQuestions)
            .ThenInclude(q => q.AnswerOptions.OrderBy(ao => ao.DisplayOrder)) // AnswerOptions have DisplayOrder
            .FirstOrDefaultAsync(p => p.Id == pageId);

        // If the page is not found, returns null.
        if (page == null)
        {
            _logger.LogWarning("Page with ID {PageId} not found.", pageId);
            return null;
        }

        // Maps the Page entity and its related data to a PageDetailDTO.
        var pageDetailDto = new PageDetailDTO
        {
            Id = page.Id,
            Title = page.Title,
            Content = page.Content,
            ParentPageId = page.ParentPageId,
            AssociatedQuestions = page
                .AssociatedQuestions.Select(q => new QuestionDTO
                {
                    Id = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Options = q
                        .AnswerOptions.Select(ao => new AnswerOptionDTO
                        {
                            Id = ao.AnswerOptionId,
                            OptionText = ao.OptionText,
                            IsCorrect = ao.IsCorrect,
                            DisplayOrder = ao.DisplayOrder,
                        })
                        .ToList(),
                })
                .ToList(),
        };

        // Returns the PageDetailDTO.
        return pageDetailDto;
    }

    // Asynchronously retrieves the display order of pages within a section.
    public async Task<List<int>> GetSectionPageOrderAsync(int currentPageId)
    {
        // Attempts to find the current page by its ID.
        var currentPage = await _context.Pages.FindAsync(currentPageId);
        // If the current page is not found, returns an empty list.
        if (currentPage == null)
        {
            _logger.LogWarning(
                "Current page with ID {CurrentPageId} not found for section order.",
                currentPageId
            );
            return new List<int>();
        }

        // Determine the ID of the page whose children (and their descendants) form the section.
        // If currentPage is a root page (no parent), its children form the section.
        // If currentPage is a child page, its parent's children (i.e., currentPage and its siblings) form the section.
        int? sectionParentId = currentPage.ParentPageId ?? currentPage.Id;

        _logger.LogDebug(
            "Fetching ordered descendants for section parent ID: {SectionParentId}",
            sectionParentId
        );

        var orderedPageIds = new List<int>();
        await FetchOrderedDescendantIdsAsync(sectionParentId, orderedPageIds);

        return orderedPageIds;
    }

    // Private recursive helper method to fetch and order descendant page IDs.
    private async Task FetchOrderedDescendantIdsAsync(int? parentId, List<int> orderedPageIds)
    {
        // Fetches direct children of the given parentId, ordered by their DisplayOrder.
        var children = await _context
            .Pages.Where(p => p.ParentPageId == parentId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

        // For each child found:
        foreach (var child in children)
        {
            // Add the child's ID to the list.
            orderedPageIds.Add(child.Id);
            // Recursively call this method to fetch and add all descendants of the current child.
            await FetchOrderedDescendantIdsAsync(child.Id, orderedPageIds);
        }
    }

    // Asynchronously creates a new learning page.
    public async Task<PageDetailDTO> CreatePageAsync(PageCreateRequestDTO createRequest)
    {
        var newPage = new Page
        {
            Title = createRequest.Title,
            Content = createRequest.Content,
            ParentPageId = createRequest.ParentPageId,
            DisplayOrder = createRequest.DisplayOrder,
            AssociatedQuestions = (
                createRequest.AssociatedQuestions ?? new List<QuestionCreateOrUpdateDTO>()
            )
                .Select(qDto => new Question
                {
                    QuestionText = qDto.QuestionText,
                    AnswerOptions = (qDto.Options ?? new List<AnswerOptionCreateOrUpdateDTO>())
                        .Select(optDto => new AnswerOption
                        {
                            OptionText = optDto.OptionText,
                            IsCorrect = optDto.IsCorrect,
                            DisplayOrder = optDto.DisplayOrder,
                        })
                        .OrderBy(ao => ao.DisplayOrder)
                        .ToList(),
                })
                .ToList(),
        };

        _context.Pages.Add(newPage);
        await _context.SaveChangesAsync();

        // Re-fetch to get all generated IDs and ensure consistency with GetPageDetailAsync formatting.
        // The null forgiveness operator (!) is used because we expect the page to be found after creation.
        return (await GetPageDetailAsync(newPage.Id))!;
    }

    // Asynchronously updates an existing learning page.
    public async Task<bool> UpdatePageAsync(int pageId, PageUpdateRequestDTO updateRequest)
    {
        var existingPage = await _context
            .Pages.Include(p => p.AssociatedQuestions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(p => p.Id == pageId);

        if (existingPage == null)
        {
            _logger.LogWarning("Page with ID {PageId} not found for update.", pageId);
            return false; // Indicates "not found"
        }

        // Update scalar properties
        existingPage.Title = updateRequest.Title;
        existingPage.Content = updateRequest.Content;
        existingPage.ParentPageId = updateRequest.ParentPageId;
        existingPage.DisplayOrder = updateRequest.DisplayOrder;

        var updatedQuestionDtos =
            updateRequest.AssociatedQuestions ?? new List<QuestionCreateOrUpdateDTO>();
        var existingQuestions = existingPage.AssociatedQuestions.ToList();

        // Identify questions to remove
        var questionsToRemove = existingQuestions
            .Where(eq =>
                !updatedQuestionDtos.Any(uqDto => uqDto.Id == eq.QuestionId && uqDto.Id != 0)
            )
            .ToList();

        if (questionsToRemove.Any())
        {
            foreach (var qToRemove in questionsToRemove)
            {
                _context.AnswerOptions.RemoveRange(qToRemove.AnswerOptions); // Explicitly remove options
            }
            _context.Questions.RemoveRange(questionsToRemove);
        }

        foreach (var qDto in updatedQuestionDtos)
        {
            Question? existingQuestion =
                (qDto.Id != 0)
                    ? existingQuestions.FirstOrDefault(q => q.QuestionId == qDto.Id)
                    : null;

            if (existingQuestion != null) // Update existing question
            {
                existingQuestion.QuestionText = qDto.QuestionText;
                var updatedOptionDtos = qDto.Options ?? new List<AnswerOptionCreateOrUpdateDTO>();
                var existingOptions = existingQuestion.AnswerOptions.ToList();

                var optionsToRemove = existingOptions
                    .Where(eo =>
                        !updatedOptionDtos.Any(uoDto =>
                            uoDto.Id == eo.AnswerOptionId && uoDto.Id != 0
                        )
                    )
                    .ToList();
                if (optionsToRemove.Any())
                {
                    _context.AnswerOptions.RemoveRange(optionsToRemove);
                }

                foreach (var optDto in updatedOptionDtos)
                {
                    AnswerOption? existingOption =
                        (optDto.Id != 0)
                            ? existingOptions.FirstOrDefault(ao => ao.AnswerOptionId == optDto.Id)
                            : null;

                    if (existingOption != null) // Update existing option
                    {
                        existingOption.OptionText = optDto.OptionText;
                        existingOption.IsCorrect = optDto.IsCorrect;
                        existingOption.DisplayOrder = optDto.DisplayOrder;
                    }
                    else // Add new option
                    {
                        var newOption = new AnswerOption
                        {
                            OptionText = optDto.OptionText,
                            IsCorrect = optDto.IsCorrect,
                            DisplayOrder = optDto.DisplayOrder,
                            QuestionId = existingQuestion.QuestionId,
                        };
                        existingQuestion.AnswerOptions.Add(newOption);
                    }
                }
                existingQuestion.AnswerOptions = existingQuestion
                    .AnswerOptions.OrderBy(ao => ao.DisplayOrder)
                    .ToList();
            }
            else // Add new question
            {
                var newQuestion = new Question
                {
                    QuestionText = qDto.QuestionText,
                    PageId = existingPage.Id,
                    AnswerOptions = (qDto.Options ?? new List<AnswerOptionCreateOrUpdateDTO>())
                        .Select(optDto => new AnswerOption
                        {
                            OptionText = optDto.OptionText,
                            IsCorrect = optDto.IsCorrect,
                            DisplayOrder = optDto.DisplayOrder,
                        })
                        .OrderBy(ao => ao.DisplayOrder)
                        .ToList(),
                };
                existingPage.AssociatedQuestions.Add(newQuestion);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while updating page with ID {PageId}.", pageId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page with ID {PageId}.", pageId);
            return false;
        }
    }

    // Asynchronously deletes a learning page.
    public async Task<bool> DeletePageAsync(int pageId)
    {
        var pageToDelete = await _context
            .Pages.Include(p => p.AssociatedQuestions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(p => p.Id == pageId);

        if (pageToDelete == null)
        {
            _logger.LogWarning("Page with ID {PageId} not found for deletion.", pageId);
            return false;
        }

        foreach (var question in pageToDelete.AssociatedQuestions.ToList())
        {
            _context.AnswerOptions.RemoveRange(question.AnswerOptions);
            _context.Questions.Remove(question);
        }
        _context.Pages.Remove(pageToDelete);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting page with ID {PageId}.", pageId);
            return false;
        }
    }
}
