using backend.DTO.LearningEnvironment;
using backend.Services.LearningEnvironment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnswersController : ControllerBase
{
    private readonly IAnswerService _answerService;

    public AnswersController(IAnswerService answerService)
    {
        _answerService = answerService;
    }

    // Defines an HTTP POST endpoint at "api/answers/check" for checking user-submitted answers.
    [HttpPost("check")]
    [Authorize]
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
