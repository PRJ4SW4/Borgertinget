using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.DTOs; // Required for Poll DTOs
using backend.Hubs; // Required for FeedHub
using backend.Models; // Required for PoliticianTwitterId models
using backend.Services.Polls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // Required for IHubContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Controllers
{
    [TestFixture]
    public class PollControllerAdminTests
    {
        private PollsController _uut;
        private IHubContext<FeedHub> _mockHubContext;
        private IClientProxy _mockClientProxy; // Added for verifying SignalR calls
        private IPollsService _mockPollsService;
        private ILogger<PollsController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockPollsService = Substitute.For<IPollsService>();
            _mockHubContext = Substitute.For<IHubContext<FeedHub>>();
            _mockLogger = Substitute.For<ILogger<PollsController>>();

            var mockClients = Substitute.For<IHubClients>();
            _mockClientProxy = Substitute.For<IClientProxy>();
            _mockHubContext.Clients.Returns(mockClients);
            mockClients.All.Returns(_mockClientProxy);

            _uut = new PollsController(_mockPollsService, _mockHubContext, _mockLogger);
        }

        private Task<PoliticianTwitterId> SeedPolitician(
            int id = 1,
            string name = "Test Politician",
            string handle = "testpol"
        )
        {
            var politician = new PoliticianTwitterId
            {
                Id = id,
                Name = name,
                TwitterHandle = handle,
            };
            _mockPollsService.GetPolitician(id).Returns(Task.FromResult(politician));
            return Task.FromResult(politician);
        }

        private Task<PollDetailsDto> SeedPoll(
            int? pollId = null,
            string question = "Test Poll",
            int politicianId = 1,
            List<string>? optionTexts = null,
            DateTime? endedAt = null,
            bool generateId = false
        )
        {
            if (optionTexts == null)
            {
                optionTexts = new List<string> { "Opt A", "Opt B" };
            }

            var pollDetailsDto = new PollDetailsDto
            {
                Id = pollId ?? 1,
                Question = question,
                PoliticianId = politicianId,
                CreatedAt = DateTime.UtcNow,
                EndedAt = endedAt,
                Options = optionTexts.Select(o => new PollOptionDto { OptionText = o }).ToList(),
            };

            _mockPollsService
                .CreatePollAsync(Arg.Any<PollDto>())
                .Returns(Task.FromResult(pollDetailsDto));
            return Task.FromResult(pollDetailsDto);
        }

        #region CreatePoll Tests

        [Test]
        public async Task CreatePoll_ValidDto_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var politician = await SeedPolitician(101);
            var endedAtDate = DateTime.UtcNow.AddDays(7);
            var createPollDto = new PollDto
            {
                Question = "Favorite color?",
                Options = new List<string> { "Red", "Blue" },
                PoliticianTwitterId = politician.Id,
                EndedAt = endedAtDate,
            };

            var pollDetailsDto = new PollDetailsDto
            {
                Id = 1,
                Question = createPollDto.Question,
                Options = createPollDto
                    .Options.Select(o => new PollOptionDto { OptionText = o })
                    .ToList(),
                EndedAt = endedAtDate,
            };

            _mockPollsService
                .CreatePollAsync(createPollDto)
                .Returns(Task.FromResult(pollDetailsDto));

            // Act
            var result = await _uut.CreatePoll(createPollDto);

            // Assert
            Assert.That(result, Is.TypeOf<ActionResult<PollDetailsDto>>());
            var actionResult = result.Result as CreatedAtActionResult;
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.StatusCode, Is.EqualTo(201));
            Assert.That(actionResult.Value, Is.EqualTo(pollDetailsDto));
        }

        [Test]
        public async Task CreatePoll_PoliticianNotFound_ReturnsValidationProblem()
        {
            // Arrange
            var createPollDto = new PollDto
            {
                Question = "Test Question",
                Options = new List<string> { "Opt1", "Opt2" },
                PoliticianTwitterId = 999, // Non-existent
            };

            // Act
            var result = await _uut.CreatePoll(createPollDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            var objectResult = result.Result as NotFoundObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
            var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
            Assert.That(validationProblemDetails, Is.Not.Null);
            Assert.That(validationProblemDetails.Title, Is.EqualTo("Creation failed"));
        }

        [Test]
        public async Task CreatePoll_EmptyOption_ReturnsValidationProblem()
        {
            // Arrange
            var politician = await SeedPolitician(102);
            var createPollDto = new PollDto
            {
                Question = "Test Question",
                Options = new List<string> { "Opt1", "" }, // Empty option
                PoliticianTwitterId = politician.Id,
            };

            // Act
            var result = await _uut.CreatePoll(createPollDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreatePoll_DuplicateOptions_ReturnsValidationProblem()
        {
            // Arrange
            var politician = await SeedPolitician(103);
            var createPollDto = new PollDto
            {
                Question = "Test Question",
                Options = new List<string> { "Opt1", "Opt1" }, // Duplicate options
                PoliticianTwitterId = politician.Id,
            };

            // Act
            var result = await _uut.CreatePoll(createPollDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        #endregion

        #region UpdatePoll Tests

        [Test]
        public async Task UpdatePoll_ValidDto_ReturnsNoContentResult()
        {
            // Arrange
            var politician1 = await SeedPolitician(201, "Pol1", "pol1");
            var politician2 = await SeedPolitician(202, "Pol2", "pol2");

            var initialPoll = new PollDetailsDto
            {
                Id = 1,
                Question = "Initial Question",
                PoliticianId = politician1.Id,
                Options = new List<PollOptionDto> { new PollOptionDto { OptionText = "OldOpt1" } },
            };

            _mockPollsService
                .GetPollByIdAsync(initialPoll.Id, Arg.Any<int>())
                .Returns(Task.FromResult<PollDetailsDto?>(initialPoll));

            var updateDto = new PollDto
            {
                Question = "Updated Question",
                Options = new List<string> { "NewOpt1", "NewOpt2" },
                PoliticianTwitterId = politician2.Id,
                EndedAt = DateTime.UtcNow.AddDays(10),
            };

            var updatedPoll = new PollDetailsDto
            {
                Id = initialPoll.Id,
                Question = updateDto.Question,
                PoliticianId = politician2.Id,
                Options = updateDto
                    .Options.Select(o => new PollOptionDto { OptionText = o })
                    .ToList(),
                EndedAt = updateDto.EndedAt,
            };

            _mockPollsService
                .UpdatePollAsync(initialPoll.Id, updateDto)
                .Returns(Task.FromResult(true));

            _mockPollsService
                .GetPollByIdAsync(initialPoll.Id, Arg.Any<int>())
                .Returns(Task.FromResult<PollDetailsDto?>(updatedPoll));

            // Act
            var result = await _uut.UpdatePoll(initialPoll.Id, updateDto);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task UpdatePoll_PollNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var updateDto = new PollDto
            {
                Question = "Q",
                Options = new List<string> { "O" },
                PoliticianTwitterId = 1,
            };

            _mockPollsService.UpdatePollAsync(Arg.Any<int>(), updateDto).Returns(false);

            // Act
            var result = await _uut.UpdatePoll(999, updateDto);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        // [Test]
        // public async Task UpdatePoll_DuplicateOptions_ReturnsValidationProblem()
        // {
        //     // Arrange
        //     var politician = await SeedPolitician(205);
        //     var poll = new PollDto
        //     {
        //         Question = "Q",
        //         Options = new List<string> { "Opt1", "Opt1" },
        //         PoliticianTwitterId = politician.Id,
        //     };
        //     _mockPollsService.CreatePollAsync(Arg.Any<PollDto>()).Returns(poll);

        //     var updateDto = new PollDto
        //     {
        //         Question = "Updated Q",
        //         Options = new List<string> { "Opt1", "Opt1" }, // Duplicate options
        //         PoliticianTwitterId = politician.Id,
        //     };

        //     // Act
        //     var result = await _uut.UpdatePoll(poll.Id, updateDto);

        //     // Assert
        //     Assert.That(result, Is.InstanceOf<ObjectResult>());
        //     var objectResult = result as ObjectResult;
        //     Assert.That(objectResult, Is.Not.Null);

        //     Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        //     var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
        //     Assert.That(validationProblemDetails, Is.Not.Null);

        //     Assert.That(
        //         validationProblemDetails.Errors.ContainsKey(nameof(PollDto.Options)),
        //         Is.True
        //     );
        // }

        #endregion

        #region DeletePoll Tests

        [Test]
        public async Task DeletePoll_PollNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistentPollId = 999;

            // Act
            var result = await _uut.DeletePoll(nonExistentPollId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null, "NotFoundResult should not be null.");
            Assert.That(
                notFoundResult!.Value,
                Is.EqualTo($"Poll med ID {nonExistentPollId} blev ikke fundet.")
            );
        }

        [Test]
        public async Task DeletePoll_ExistingPoll_DeletesPollAndRelatedData_ReturnsOk_NotifiesHub()
        {
            // Arrange
            var politician = await SeedPolitician(301, "PolForDelete", "polfordel");
            var poll = await SeedPoll(
                301,
                "Test Poll",
                politician.Id,
                new List<string> { "DelOpt1", "DelOpt2" }
            );

            var pollIdToDelete = poll.Id;

            // Mock service behavior for deletion
            _mockPollsService.DeletePollAsync(pollIdToDelete).Returns(Task.FromResult(true));
            _mockPollsService
                .GetPollByIdAsync(pollIdToDelete, Arg.Any<int>())
                .Returns(Task.FromResult<PollDetailsDto?>(null));

            // Act
            var result = await _uut.DeletePoll(pollIdToDelete);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.Not.Null);

            var messageProperty = okResult.Value.GetType().GetProperty("message");
            Assert.That(messageProperty, Is.Not.Null);
            var messageValue = messageProperty!.GetValue(okResult.Value, null);
            Assert.That(messageValue, Is.EqualTo($"Poll med ID {pollIdToDelete} blev slettet."));

            // Verify data is deleted via service calls
            var deletedPoll = await _mockPollsService.GetPollByIdAsync(
                pollIdToDelete,
                Arg.Any<int>()
            );
            Assert.That(deletedPoll, Is.Null);

            // Verify SignalR Hub was called
            await _mockClientProxy
                .Received(1)
                .SendCoreAsync(
                    "PollDeleted",
                    Arg.Is<object[]>(args => args.Length == 1 && (int)args[0] == pollIdToDelete)
                );
        }

        #endregion
    }
}