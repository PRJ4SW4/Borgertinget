using backend.DTO.LearningEnvironment;
using backend.Services.LearningEnvironment;
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
    [Authorize]
    public async Task<ActionResult<IEnumerable<PageSummaryDTO>>> GetPagesStructure()
    {
        // Calls the service to get the page structure.
        var structure = await _pageService.GetPagesStructureAsync();
        // Returns an HTTP 200 OK response with the page structure.
        return Ok(structure);
    }

    // Defines an HTTP GET endpoint at "api/pages/{id}" to retrieve the details of a specific page.
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PageDetailDTO>> GetPage(int id)
    {
        // Calls the service to get page details.
        var pageDetail = await _pageService.GetPageDetailAsync(id);

        // If the page detail is null (page not found), returns an HTTP 404 Not Found response.
        if (pageDetail == null)
        {
            return NotFound($"Page with ID {id} not found.");
        }

        // Returns an HTTP 200 OK response with the page details.
        return Ok(pageDetail);
    }

    // Defines an HTTP GET endpoint at "api/pages/{id}/sectionorder"
    // to retrieve the ordered list of page IDs within the same section as the given page.
    [HttpGet("{id}/sectionorder")]
    [Authorize]
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
        // Calls the service to create a page.
        var newPageDto = await _pageService.CreatePageAsync(createRequest);
        // Assuming CreatePageAsync now correctly returns PageDetailDTO and handles all logic
        return CreatedAtAction(nameof(GetPage), new { id = newPageDto.Id }, newPageDto);
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
        // Calls the service to update the page by id.
        var success = await _pageService.UpdatePageAsync(id, updateRequest);

        if (!success)
        {
            // Consider if UpdatePageAsync should distinguish between "not found" and "other error"
            // For now, assuming false means something went wrong, potentially "not found" or concurrency.
            // The service layer should log specifics. Here, we might return NotFound or a generic error.
            // If the service specifically indicates "not found" (e.g., by throwing a specific exception or returning a more detailed result object),
            // we could return NotFound(). For now, NoContent() if successful, or a problem if not.
            return NotFound();
        }

        return NoContent(); // Standard response for successful PUT update with no content to return
    }

    // DELETE: api/pages/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePage(int id)
    {
        // Calls the service to delete the page by id.
        var success = await _pageService.DeletePageAsync(id);

        if (!success)
        {
            // Similar to PUT, service should log. Controller returns NotFound if deletion failed (likely due to item not existing).
            return NotFound();
        }

        return NoContent(); // Standard response for successful DELETE
    }
}
