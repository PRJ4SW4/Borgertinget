namespace backend.Repositories.LearningEnvironment;

using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.LearningEnvironment;

public interface ILearningPageRepository
{
    Task<IEnumerable<Page>> GetAllPagesAsync();
    Task<Page?> GetPageByIdAsync(int pageId);
    Task<Page?> GetPageWithDetailsAsync(int pageId); // Includes questions and options
    Task<IEnumerable<Page>> GetChildPagesOrderedAsync(int? parentPageId);
    Task AddPageAsync(Page page);
    void UpdatePage(Page page); // EF Core tracks changes, SaveChangesAsync called separately
    void RemoveQuestion(Question question);
    void RemoveAnswerOption(AnswerOption option);
    void RemoveRangeAnswerOptions(IEnumerable<AnswerOption> options);
    void RemoveRangeQuestions(IEnumerable<Question> questions);
    void RemovePage(Page page);
    Task<int> SaveChangesAsync();
}
