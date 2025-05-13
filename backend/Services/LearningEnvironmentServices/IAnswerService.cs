namespace backend.Services.LearningEnvironmentServices;

using System.Threading.Tasks;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;

// Defines a contract for services that handle answer-related operations.
public interface IAnswerService
{
    // Asynchronously checks a user-submitted answer against the correct answer.
    // Returns a DTO indicating if the answer was correct, or null if the input was invalid.
    Task<AnswerCheckResponseDTO?> CheckAnswerAsync(AnswerCheckRequestDTO request);
}
