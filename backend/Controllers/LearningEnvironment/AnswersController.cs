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
    private readonly DataContext _context;

    public AnswersController(DataContext context)
    {
        _context = context; // Injecting the DataContext
    }

    // POST: api/answers/check
    [HttpPost("check")]
    public async Task<ActionResult<AnswerCheckResponseDTO>> CheckAnswer(
        AnswerCheckRequestDTO request
    )
    {
        // Find the answer option the user selected in the database
        var selectedOption = await _context.AnswerOptions.FirstOrDefaultAsync(opt =>
            opt.AnswerOptionId == request.SelectedAnswerOptionId
        );

        // Validate: Check if option exists and belongs to the correct question
        if (selectedOption == null || selectedOption.QuestionId != request.QuestionId)
        {
            // Return BadRequest or NotFound if the submitted data is invalid
            // Using BadRequest might be better as it indicates a client-side issue (sending bad data)
            return BadRequest("Invalid answer option or question ID mismatch.");
        }

        // Prepare the response based on the IsCorrect property
        var response = new AnswerCheckResponseDTO
        {
            IsCorrect = selectedOption.IsCorrect,
            // You could potentially fetch and include the correct answer ID here if needed:
            // CorrectAnswerOptionId = await _context.AnswerOptions
            //    .Where(o => o.QuestionId == request.QuestionId && o.IsCorrect)
            //    .Select(o => o.AnswerOptionId)
            //    .FirstOrDefaultAsync() ?? 0 // Handle case where no correct answer is marked
        };

        return Ok(response);
    }
}
