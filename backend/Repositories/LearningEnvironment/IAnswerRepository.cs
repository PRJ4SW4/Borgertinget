namespace backend.Repositories.LearningEnvironment;

using System.Threading.Tasks;
using backend.Models.LearningEnvironment;

public interface IAnswerRepository
{
    Task<AnswerOption?> GetAnswerOptionByIdAsync(int answerOptionId);
}
