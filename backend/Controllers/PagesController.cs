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
            return NotFound();
        }

        var pageDetail = new PageDetailDto
        {
            Id = page.Id,
            Title = page.Title,
            Content = page.Content, // Pass the raw Markdown
            ParentPageId = page.ParentPageId,
        };

        return Ok(pageDetail);
    }

    // --- Add POST, PUT, DELETE endpoints later ---
    // Add [Authorize(Roles = "Admin")] attribute later
}
