// /backend/Controllers/LearningEnvironment/AnswersController.cs
namespace backend.Controllers;

using System.Threading.Tasks;
using backend.Data;
using backend.DTO.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AnswersController : ControllerBase
{
    // A private readonly field to hold the DataContext instance, enabling database interactions.
    private readonly DataContext _context;

    // Constructor for the AnswersController, injecting the DataContext via dependency injection.
    public AnswersController(DataContext context)
    {
        // Assigns the injected DataContext instance to the private field for use within the controller.
        _context = context;
    }

    // Defines an HTTP POST endpoint at "api/answers/check" for checking user-submitted answers.
    [HttpPost("check")]
    public async Task<ActionResult<AnswerCheckResponseDTO>> CheckAnswer(
        AnswerCheckRequestDTO request
    )
    {
        // Asynchronously retrieves the selected answer option from the database based on the provided AnswerOptionId.
        var selectedOption = await _context.AnswerOptions.FirstOrDefaultAsync(opt =>
            opt.AnswerOptionId == request.SelectedAnswerOptionId
        );

        // Validates that the selected answer option exists and belongs to the question specified in the request.
        if (selectedOption == null || selectedOption.QuestionId != request.QuestionId)
        {
            // Returns an HTTP 400 Bad Request response if the selected option is invalid or doesn't match the question.
            return BadRequest("Invalid answer option or question ID mismatch.");
        }

        // Creates a new AnswerCheckResponseDTO to encapsulate the result of the answer check.
        var response = new AnswerCheckResponseDTO
        {
            // Sets the IsCorrect property of the response DTO based on the IsCorrect property of the selected answer option.
            IsCorrect = selectedOption.IsCorrect,
        };

        // Returns an HTTP 200 OK response containing the AnswerCheckResponseDTO, indicating whether the answer was correct.
        return Ok(response);
    }
}
