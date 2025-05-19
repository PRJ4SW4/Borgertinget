namespace backend.Repositories.LearningEnvironment;

using System.Threading.Tasks;
using backend.Data;
using backend.Models.LearningEnvironment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class AnswerRepository : IAnswerRepository
{
    private readonly DataContext _context;
    private readonly ILogger<AnswerRepository> _logger;

    public AnswerRepository(DataContext context, ILogger<AnswerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AnswerOption?> GetAnswerOptionByIdAsync(int answerOptionId)
    {
        _logger.LogInformation(
            "Fetching answer option by ID: {AnswerOptionId} from repository.",
            answerOptionId
        );
        return await _context.AnswerOptions.FirstOrDefaultAsync(opt =>
            opt.AnswerOptionId == answerOptionId
        );
    }
}
