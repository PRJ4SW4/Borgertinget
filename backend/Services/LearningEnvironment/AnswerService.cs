namespace backend.Services.LearningEnvironment;

using System.Threading.Tasks;
using backend.Data;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Service responsible for handling logic related to checking answers.
public class AnswerService : IAnswerService
{
    // A private readonly field to hold the DataContext instance, enabling database interactions.
    private readonly DataContext _context;

    // A private readonly field for logging.
    private readonly ILogger<AnswerService> _logger;

    // Constructor for the AnswerService, injecting the DataContext and ILogger.
    public AnswerService(DataContext context, ILogger<AnswerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Asynchronously checks a user-submitted answer.
    public async Task<AnswerCheckResponseDTO?> CheckAnswerAsync(AnswerCheckRequestDTO request)
    {
        // Asynchronously retrieves the selected answer option from the database based on the provided AnswerOptionId.
        var selectedOption = await _context.AnswerOptions.FirstOrDefaultAsync(opt =>
            opt.AnswerOptionId == request.SelectedAnswerOptionId
        );

        // Validates that the selected answer option exists and belongs to the question specified in the request.
        if (selectedOption == null || selectedOption.QuestionId != request.QuestionId)
        {
            _logger.LogWarning(
                "Attempt to check answer with invalid option ID {SelectedAnswerOptionId} for question ID {QuestionId}, or mismatch.",
                request.SelectedAnswerOptionId,
                request.QuestionId
            );
            // Returns null if the selected option is invalid or doesn't match the question,
            // allowing the controller to return a BadRequest.
            return null;
        }

        // Creates a new AnswerCheckResponseDTO to encapsulate the result of the answer check.
        var response = new AnswerCheckResponseDTO
        {
            // Sets the IsCorrect property of the response DTO based on the IsCorrect property of the selected answer option.
            IsCorrect = selectedOption.IsCorrect,
        };

        // Returns the response DTO.
        return response;
    }
}
