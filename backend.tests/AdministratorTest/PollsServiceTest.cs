using backend.DTOs;
using backend.Models;
using backend.Repositories.Polls;
using backend.Services.Polls;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Services;

[TestFixture]
public class PollsServiceTest
{
    private PollsService _uut;
    private IPollsRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IPollsRepository>();
        _uut = new PollsService(_repository);
    }

    #region CreatePollAsync Tests

    [Test]
    public async Task CreatePollAsync_ShouldReturnPollDetailsDto_WhenCreationIsSuccessful()
    {
        // Arrange
        var createDto = new PollDto
        {
            Question = "What is your favorite color?",
            Options = new List<string> { "Red", "Blue", "Green" },
            PoliticianTwitterId = 456,
            EndedAt = DateTime.UtcNow.AddDays(10),
        };

        var createdPoll = new Poll
        {
            Id = 1,
            Question = createDto.Question,
            PoliticianTwitterId = createDto.PoliticianTwitterId,
            CreatedAt = DateTime.UtcNow,
            EndedAt = createDto.EndedAt,
            Options = createDto.Options.Select(o => new PollOption { OptionText = o }).ToList(),
        };

        var politician = new PoliticianTwitterId
        {
            Id = createDto.PoliticianTwitterId,
            Name = "John Doe",
            TwitterHandle = "@JohnDoe",
        };

        _repository.CreatePollAsync(Arg.Any<Poll>()).Returns(createdPoll);
        _repository.GetPoliticianByIdAsync(createDto.PoliticianTwitterId).Returns(politician);

        // Act
        var result = await _uut.CreatePollAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdPoll.Id));
        Assert.That(result.Question, Is.EqualTo(createdPoll.Question));
        Assert.That(result.PoliticianId, Is.EqualTo(politician.Id));
        Assert.That(result.PoliticianName, Is.EqualTo(politician.Name));
        Assert.That(result.PoliticianHandle, Is.EqualTo(politician.TwitterHandle));
        Assert.That(result.Options.Count, Is.EqualTo(createdPoll.Options.Count));
    }

    #endregion

    #region UpdatePollAsync Tests

    [Test]
    public async Task UpdatePollAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var pollId = 1;
        var updateDto = new PollDto
        {
            Question = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            Options = new List<string> { "Option 1", "Option 2" },
            PoliticianTwitterId = 123,
            EndedAt = DateTime.UtcNow.AddDays(7),
        };
        var poll = new Poll { Id = pollId };

        _repository.GetPollByIdAsync(pollId).Returns(poll);
        _repository.UpdatePollAsync(poll).Returns(true);
        _repository.SaveChangesAsync().Returns(1);

        // Act
        var result = await _uut.UpdatePollAsync(pollId, updateDto);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(poll.Question, Is.EqualTo(updateDto.Question));
    }

    [Test]
    public async Task UpdatePollAsync_ShouldReturnFalse_WhenPollNotFound()
    {
        // Arrange
        var pollId = 1;
        var updateDto = new PollDto
        {
            Question = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            Options = new List<string> { "Option 1", "Option 2" },
            PoliticianTwitterId = 123,
            EndedAt = DateTime.UtcNow.AddDays(7),
        };

        _repository.GetPollByIdAsync(pollId).Returns((Poll?)null);

        // Act
        var result = await _uut.UpdatePollAsync(pollId, updateDto);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region DeletePollAsync Tests

    [Test]
    public async Task DeletePollAsync_ShouldReturnTrue_WhenDeletionIsSuccessful()
    {
        // Arrange
        var pollId = 1;
        var poll = new Poll { Id = pollId };

        _repository.GetPollByIdAsync(pollId).Returns(poll);
        _repository.SaveChangesAsync().Returns(1);

        // Act
        var result = await _uut.DeletePollAsync(pollId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeletePollAsync_ShouldReturnFalse_WhenPollNotFound()
    {
        // Arrange
        var pollId = 1;

        _repository.GetPollByIdAsync(pollId).Returns((Poll?)null);

        // Act
        var result = await _uut.DeletePollAsync(pollId);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion
}
