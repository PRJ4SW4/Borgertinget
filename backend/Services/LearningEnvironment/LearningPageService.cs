namespace backend.Services.LearningEnvironment;

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
    // Returns a PageDetailDTO, or null if the page is not found.
    // Will call the BuildTraversalOrderRecursiveInMemory method to get the order of pages.
    // This is done every time a page is requested, which is not optimal.
    // Ideally, this would be cached or stored in the database. But those options come with their own struggles ;(
    // So, for now, this is the simplest and most straightforward way to do it. T2his should work for all but very large page trees.
    public async Task<PageDetailDTO?> GetPageDetailAsync(int pageId)
    {
        // 1. Fetch the current page with its details including associated questions and their options.
        //    The Include and ThenInclude methods are used to eagerly load related entities.
        var page = await _context
            .Pages.Include(p => p.AssociatedQuestions)
            .ThenInclude(q => q.AnswerOptions.OrderBy(ao => ao.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == pageId);

        if (page == null)
        {
            _logger.LogWarning("Page with ID {PageId} not found.", pageId);
            return null;
        }

        // 2. Determine the ultimate root page of the current page
        //    And fetch all pages to build the tree structure efficiently in memory.
        List<Page> allDbPages = await _context.Pages.AsNoTracking().ToListAsync(); // AsNoTracking, practically means read-only, we cant modify these entities
        // Just better performance as we don't need to change the state of these entities

        Page ultimateRootPageEntity = page; // Start with the current page
        if (page.ParentPageId.HasValue) // If the page has a parent
        {
            // Traverse upwards to find the actual root
            var currentParentIdInPath = page.ParentPageId;
            while (currentParentIdInPath.HasValue)
            {
                var parentInPath = allDbPages.FirstOrDefault(p => // We find the parent in our retrieved list of all pages
                    p.Id == currentParentIdInPath.Value
                );
                if (parentInPath == null) // We already checked if the page was null, so this is a parent that
                // doesn't exist which would not be great if it somehow reaches this
                {
                    _logger.LogWarning(
                        "Data inconsistency: Parent page with ID {ParentId} not found for page {PageId}.",
                        currentParentIdInPath.Value,
                        ultimateRootPageEntity.Id
                    );
                    // Fallback: Treat current 'ultimateRootPageEntity' as root if path breaks. This page would be an orphan.
                    // Or, if 'page' was the starting point, and its parent wasn't found, 'page' itself might be treated as root in this broken scenario.
                    // For safety, i'll stick with the highest valid ancestor found.
                    break;
                }
                ultimateRootPageEntity = parentInPath;
                currentParentIdInPath = parentInPath.ParentPageId; // Move up the tree
            }
        }
        // At this point, ultimateRootPageEntity is the highest ancestor found for this learning section.

        // 3. Now we start at that root and generate the traversal order list for the identified root's tree
        List<int> traversalOrder = new List<int>();
        if (ultimateRootPageEntity != null)
        {
            BuildTraversalOrderRecursiveInMemory( // Here we call the recursive function below to build the order. Shoutout to the data structures an algorithms course ;)
                // this is a depth-first traversal
                ultimateRootPageEntity,
                traversalOrder,
                allDbPages
            );
        }
        else
        {
            // This case should ideally not be hit if 'page' was found and 'allDbPages' contains it.
            // If ultimateRootPageEntity ended up null due to some very extreme data corruption ;() where 'page' had a ParentPageId
            // but no such parent existed in allDbPages, then i'm just gonna assume we only have the current page to work with.
            _logger.LogWarning(
                "Could not determine a valid root for page {PageId}. Navigation will be limited.",
                pageId
            );
            if (page != null)
                traversalOrder.Add(page.Id);
        }

        // 4. Determine Previous and Next page IDs from the traversal list
        int? previousPageIdInTraversal = null;
        int? nextPageIdInTraversal = null;

        if (traversalOrder.Any()) // Make sure we actually got a traversal order. If not the values will just be null and the buttons will be grayed out.
        // Gracefully handled As Fuck
        {
            int currentIndexInTraversal = traversalOrder.IndexOf(page.Id);

            if (currentIndexInTraversal > 0) // If not the first page in the traversal
            {
                previousPageIdInTraversal = traversalOrder[currentIndexInTraversal - 1]; // Set previous page ID
            }
            if (currentIndexInTraversal >= 0 && currentIndexInTraversal < traversalOrder.Count - 1) // If not the last page
            {
                nextPageIdInTraversal = traversalOrder[currentIndexInTraversal + 1]; // Set next page ID
            }
        }

        // 5. Map to DTO
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
            PreviousSiblingId = previousPageIdInTraversal,
            NextSiblingId = nextPageIdInTraversal,
        };

        return pageDetailDto;
    }

    /// Recursively builds a depth-first traversal list of page IDs for a given root's hierarchy.
    /// Uses a pre-fetched list of all pages for efficient lookups.
    private void BuildTraversalOrderRecursiveInMemory(
        Page currentPage,
        List<int> listToUse,
        List<Page> listOfallPages
    )
    {
        listToUse.Add(currentPage.Id); // First add the current page ID to the list, as this is the root

        var children = listOfallPages // Get all children of the current page from the pre-fetched list and order them by DisplayOrder.
            .Where(p => p.ParentPageId == currentPage.Id)
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        foreach (var child in children) // For each child found, recursively call this method to build the order for its subtree.
        // This is a depth-first traversal, so we go as deep as possible before backtracking.
        {
            BuildTraversalOrderRecursiveInMemory(child, orderedIdList, allSystemPages);
        }
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
