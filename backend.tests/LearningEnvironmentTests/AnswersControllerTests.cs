using backend.Controllers;
using backend.DTO.LearningEnvironment;
using backend.Services.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace backend.tests.LearningEnvironmentTests;

[TestFixture]
public class AnswersControllerTests
{
    private IAnswerService _mockAnswerService;
    private AnswersController _uut;

    [SetUp]
    public void Setup()
    {
        _mockAnswerService = Substitute.For<IAnswerService>();
        _uut = new AnswersController(_mockAnswerService);
    }

    [Test]
    public async Task CheckAnswer_ValidRequest_ReturnsOkResult_WithResponse()
    {
        // Arrange
        var request = new AnswerCheckRequestDTO { QuestionId = 1, SelectedAnswerOptionId = 1 };
        var expectedResponse = new AnswerCheckResponseDTO { IsCorrect = true };
        _mockAnswerService
            .CheckAnswerAsync(request)
            .Returns(Task.FromResult<AnswerCheckResponseDTO?>(expectedResponse));

        // Act
        var result = await _uut.CheckAnswer(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null, "okResult should not be null");
        if (okResult != null)
        {
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
        }
    }

    [Test]
    public async Task CheckAnswer_InvalidRequest_ServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new AnswerCheckRequestDTO { QuestionId = 1, SelectedAnswerOptionId = 99 };
        _mockAnswerService
            .CheckAnswerAsync(request)
            .Returns(Task.FromResult<AnswerCheckResponseDTO?>(null));

        // Act
        var result = await _uut.CheckAnswer(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
