namespace backend.Repositories.LearningEnvironment;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models.LearningEnvironment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class LearningPageRepository : ILearningPageRepository
{
    private readonly DataContext _context;
    private readonly ILogger<LearningPageRepository> _logger;

    public LearningPageRepository(DataContext context, ILogger<LearningPageRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Page>> GetAllPagesAsync()
    {
        _logger.LogInformation("Fetching all learning pages.");
        return await _context.Pages.ToListAsync();
    }

    public async Task<Page?> GetPageByIdAsync(int pageId)
    {
        _logger.LogInformation("Fetching page by ID: {PageId}.", pageId);
        return await _context.Pages.FindAsync(pageId);
    }

    public async Task<Page?> GetPageWithDetailsAsync(int pageId)
    {
        _logger.LogInformation("Fetching page with details by ID: {PageId}.", pageId);
        return await _context
            .Pages.Include(p => p.AssociatedQuestions)
            .ThenInclude(q => q.AnswerOptions.OrderBy(ao => ao.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == pageId);
    }

    public async Task<IEnumerable<Page>> GetChildPagesOrderedAsync(int? parentPageId)
    {
        _logger.LogInformation(
            "Fetching child pages for parent ID: {ParentPageId}, ordered by DisplayOrder.",
            parentPageId
        );
        return await _context
            .Pages.Where(p => p.ParentPageId == parentPageId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }

    public async Task AddPageAsync(Page page)
    {
        var sanitizedTitle = page.Title?.Replace("\n", "").Replace("\r", "");
        _logger.LogInformation("Adding new page titled: {Title}.", sanitizedTitle);
        await _context.Pages.AddAsync(page);
    }

    public void UpdatePage(Page page)
    {
        _logger.LogInformation("Marking page for update: {PageId}.", page.Id);
        _context.Pages.Update(page);
    }

    public void RemoveQuestion(Question question)
    {
        _logger.LogInformation("Marking question for removal: {QuestionId}.", question.QuestionId);
        _context.Questions.Remove(question);
    }

    public void RemoveAnswerOption(AnswerOption option)
    {
        _logger.LogInformation(
            "Marking answer option for removal: {AnswerOptionId}.",
            option.AnswerOptionId
        );
        _context.AnswerOptions.Remove(option);
    }

    public void RemoveRangeAnswerOptions(IEnumerable<AnswerOption> options)
    {
        _logger.LogInformation("Marking multiple answer options for removal.");
        _context.AnswerOptions.RemoveRange(options);
    }

    public void RemoveRangeQuestions(IEnumerable<Question> questions)
    {
        _logger.LogInformation("Marking multiple questions for removal.");
        _context.Questions.RemoveRange(questions);
    }

    public void RemovePage(Page page)
    {
        _logger.LogInformation("Marking page for removal: {PageId}.", page.Id);
        _context.Pages.Remove(page);
    }

    public async Task<int> SaveChangesAsync()
    {
        _logger.LogInformation("Saving changes to the database.");
        return await _context.SaveChangesAsync();
    }
}
