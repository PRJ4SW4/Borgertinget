using System.Threading.Tasks;
using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using backend.Services.LearningEnvironmentServices;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AnswersController : ControllerBase
{
    // A private readonly field to hold the IAnswerService instance.
    private readonly IAnswerService _answerService;

    // Constructor for the AnswersController, injecting the IAnswerService via dependency injection.
    public AnswersController(IAnswerService answerService)
    {
        // Assigns the injected IAnswerService instance to the private field.
        _answerService = answerService;
    }

    // Defines an HTTP POST endpoint at "api/answers/check" for checking user-submitted answers.
    [HttpPost("check")]
    public async Task<ActionResult<AnswerCheckResponseDTO>> CheckAnswer(
        AnswerCheckRequestDTO request
    )
    {
        // Calls the service to check the answer.
        var response = await _answerService.CheckAnswerAsync(request);

        // If the service returns null, it indicates invalid input.
        if (response == null)
        {
            // Returns an HTTP 400 Bad Request response.
            return BadRequest("Invalid answer option or question ID mismatch.");
        }

        // Returns an HTTP 200 OK response containing the AnswerCheckResponseDTO.
        return Ok(response);
    }
}
