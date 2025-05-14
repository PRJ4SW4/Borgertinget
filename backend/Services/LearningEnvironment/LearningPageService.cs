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
}
