using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using backend.Services.LearningEnvironmentServices;
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
    public async Task<ActionResult<List<int>>> GetSectionPageOrder(int id)
    {
        // Calls the service to get the section page order.
        var orderedPageIds = await _pageService.GetSectionPageOrderAsync(id);

        // If the list is empty (e.g., page not found or no pages in section),
        // it might still be a valid scenario, so we return Ok with the list.
        // The service handles logging if the page itself isn't found.
        return Ok(orderedPageIds);
    }
}
