namespace backend.Services.LearningEnvironment;

using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;

// Defines a contract for services that handle operations related to learning pages.
public interface ILearningPageService
{
    // Asynchronously retrieves the hierarchical structure of all learning pages.
    // Returns a collection of PageSummaryDTOs.
    Task<IEnumerable<PageSummaryDTO>> GetPagesStructureAsync();

    // Asynchronously retrieves the detailed content of a specific learning page.
    // Returns a PageDetailDTO, or null if the page is not found.
    Task<PageDetailDTO?> GetPageDetailAsync(int pageId);

    private void BuildTraversalOrderRecursiveInMemory(
        Page currentPageNode,
        List<int> orderedIdList,
        List<Page> allSystemPages
    );

    // Asynchronously retrieves the display order of pages within a section,
    // including the current page and its siblings, ordered correctly.
    // Returns a list of page IDs in their display order.
    Task<List<int>> GetSectionPageOrderAsync(int currentPageId);

    // Asynchronously creates a new learning page.
    Task<PageDetailDTO> CreatePageAsync(PageCreateRequestDTO createRequest);

    // Asynchronously updates an existing learning page.
    Task<bool> UpdatePageAsync(int pageId, PageUpdateRequestDTO updateRequest);

    // Asynchronously deletes a learning page.
    Task<bool> DeletePageAsync(int pageId);
}
