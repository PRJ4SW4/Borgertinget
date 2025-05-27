using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.DTOs;
using backend.Hubs;
using backend.Models;
using backend.Services.Polls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private IClientProxy _mockClientProxy;
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

        [Test]
        public async Task GetAllPolls_ReturnsAllPolls()
        {
            var polls = new List<PollSummaryDto>
            {
                new PollSummaryDto
                {
                    Id = 1,
                    Question = "Poll 1",
                    PoliticianTwitterId = 101,
                },
                new PollSummaryDto
                {
                    Id = 2,
                    Question = "Poll 2",
                    PoliticianTwitterId = 102,
                },
            };
            _mockPollsService.GetAllPollsAsync().Returns(Task.FromResult(polls));

            var result = await _uut.GetAllPolls();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var returnedPolls = okResult.Value as IEnumerable<PollSummaryDto>;
            Assert.That(returnedPolls, Is.Not.Null);
            Assert.That(returnedPolls.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllPolls_EmptyList_ReturnsEmptyResult()
        {
            var emptyList = new List<PollSummaryDto>();
            _mockPollsService.GetAllPollsAsync().Returns(Task.FromResult(emptyList));

            var result = await _uut.GetAllPolls();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var returnedPolls = okResult.Value as IEnumerable<PollSummaryDto>;
            Assert.That(returnedPolls, Is.Not.Null);
            Assert.That(returnedPolls.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetPollById_ExistingId_ReturnsPoll()
        {
            int userId = 123;
            int pollId = 42;
            var politician = await SeedPolitician(101, "TestPolitician", "testpol");
            var pollDetails = new PollDetailsDto
            {
                Id = pollId,
                Question = "Test Poll Question",
                PoliticianId = politician.Id,
                PoliticianName = politician.Name,
                CreatedAt = DateTime.UtcNow,
                Options = new List<PollOptionDto>
                {
                    new PollOptionDto
                    {
                        Id = 1,
                        OptionText = "Option A",
                        Votes = 5,
                    },
                    new PollOptionDto
                    {
                        Id = 2,
                        OptionText = "Option B",
                        Votes = 3,
                    },
                },
                TotalVotes = 8,
            };

            _mockPollsService
                .GetPollByIdAsync(pollId, userId)
                .Returns(Task.FromResult<PollDetailsDto?>(pollDetails));

            var claim = new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier,
                userId.ToString()
            );
            var claims = new List<System.Security.Claims.Claim> { claim };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var userPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = userPrincipal,
                },
            };
            var result = await _uut.GetPollById(pollId);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var returnedPoll = okResult.Value as PollDetailsDto;
            Assert.That(returnedPoll, Is.Not.Null);
            Assert.That(returnedPoll.Id, Is.EqualTo(pollId));
            Assert.That(returnedPoll.Question, Is.EqualTo("Test Poll Question"));
        }

        [Test]
        public async Task GetPollById_NonExistingId_ReturnsNotFound()
        {
            int userId = 123;
            int nonExistentPollId = 999;

            _mockPollsService
                .GetPollByIdAsync(nonExistentPollId, userId)
                .Returns(Task.FromResult<PollDetailsDto?>(null));

            var claim = new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier,
                userId.ToString()
            );
            var claims = new List<System.Security.Claims.Claim> { claim };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var userPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = userPrincipal,
                },
            };
            // Act
            var result = await _uut.GetPollById(nonExistentPollId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetPollById_ServiceThrowsException_Returns500()
        {
            int userId = 123;
            int pollId = 42;

            _mockPollsService
                .GetPollByIdAsync(pollId, userId)
                .Returns(
                    Task.FromException<PollDetailsDto?>(new System.Exception("Test exception"))
                );

            var claim = new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier,
                userId.ToString()
            );
            var claims = new List<System.Security.Claims.Claim> { claim };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var userPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = userPrincipal,
                },
            };

            var result = await _uut.GetPollById(pollId);

            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        }
    }
}
