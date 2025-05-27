using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using backend.Repositories.LearningEnvironment;
using backend.Services.LearningEnvironment;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace backend.tests.LearningEnvironmentTests;

[TestFixture]
public class AnswerServiceTests
{
    private IAnswerRepository _mockAnswerRepository;
    private ILogger<AnswerService> _mockLogger;
    private AnswerService _uut;

    [SetUp]
    public void Setup()
    {
        _mockAnswerRepository = Substitute.For<IAnswerRepository>();
        _mockLogger = Substitute.For<ILogger<AnswerService>>();
        _uut = new AnswerService(_mockAnswerRepository, _mockLogger);
    }

    [Test]
    public async Task CheckAnswerAsync_CorrectOptionAndMatchingQuestionId_ReturnsCorrectResponse()
    {
        // Arrange
        var request = new AnswerCheckRequestDTO { QuestionId = 1, SelectedAnswerOptionId = 10 };
        var answerOption = new AnswerOption
        {
            AnswerOptionId = 10,
            QuestionId = 1,
            IsCorrect = true,
        };
        _mockAnswerRepository
            .GetAnswerOptionByIdAsync(request.SelectedAnswerOptionId)
            .Returns(Task.FromResult<AnswerOption?>(answerOption));

        // Act
        var result = await _uut.CheckAnswerAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsCorrect, Is.True);
    }

    [Test]
    public async Task CheckAnswerAsync_IncorrectOptionAndMatchingQuestionId_ReturnsIncorrectResponse()
    {
        // Arrange
        var request = new AnswerCheckRequestDTO { QuestionId = 1, SelectedAnswerOptionId = 11 };
        var answerOption = new AnswerOption
        {
            AnswerOptionId = 11,
            QuestionId = 1,
            IsCorrect = false,
        };
        _mockAnswerRepository
            .GetAnswerOptionByIdAsync(request.SelectedAnswerOptionId)
            .Returns(Task.FromResult<AnswerOption?>(answerOption));

        // Act
        var result = await _uut.CheckAnswerAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsCorrect, Is.False);
    }

    [Test]
    public async Task CheckAnswerAsync_OptionNotFound_ReturnsNull()
    {
        // Arrange
        var request = new AnswerCheckRequestDTO { QuestionId = 1, SelectedAnswerOptionId = 99 };
        _mockAnswerRepository
            .GetAnswerOptionByIdAsync(request.SelectedAnswerOptionId)
            .Returns(Task.FromResult<AnswerOption?>(null));

        // Act
        var result = await _uut.CheckAnswerAsync(request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CheckAnswerAsync_OptionFoundButQuestionIdMismatch_ReturnsNull()
    {
        // Arrange
        var request = new AnswerCheckRequestDTO { QuestionId = 1, SelectedAnswerOptionId = 10 };
        var answerOption = new AnswerOption
        {
            AnswerOptionId = 10,
            QuestionId = 2,
            IsCorrect = true,
        }; // Different QuestionId
        _mockAnswerRepository
            .GetAnswerOptionByIdAsync(request.SelectedAnswerOptionId)
            .Returns(Task.FromResult<AnswerOption?>(answerOption));

        // Act
        var result = await _uut.CheckAnswerAsync(request);

        // Assert
        Assert.That(result, Is.Null);
    }
}
