namespace backend.Services.LearningEnvironment;

using System.Threading.Tasks;
using backend.DTO.LearningEnvironment;
using backend.Repositories.LearningEnvironment;
using Microsoft.Extensions.Logging;

// Service responsible for handling logic related to checking answers.
public class AnswerService : IAnswerService
{
    private readonly IAnswerRepository _answerRepository;
    private readonly ILogger<AnswerService> _logger;

    public AnswerService(IAnswerRepository answerRepository, ILogger<AnswerService> logger)
    {
        _answerRepository = answerRepository;
        _logger = logger;
    }

    public async Task<AnswerCheckResponseDTO?> CheckAnswerAsync(AnswerCheckRequestDTO request)
    {
        var selectedOption = await _answerRepository.GetAnswerOptionByIdAsync(
            request.SelectedAnswerOptionId
        );

        if (selectedOption == null || selectedOption.QuestionId != request.QuestionId)
        {
            _logger.LogWarning(
                "Attempt to check answer with invalid option ID {SelectedAnswerOptionId} for question ID {QuestionId}, or mismatch.",
                request.SelectedAnswerOptionId,
                request.QuestionId
            );
            return null;
        }

        var response = new AnswerCheckResponseDTO { IsCorrect = selectedOption.IsCorrect };

        return response;
    }
}
