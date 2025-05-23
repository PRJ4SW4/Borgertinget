// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using backend.Controllers;
// using backend.Data;
// using backend.DTOs; // Required for Poll DTOs
// using backend.Hubs; // Required for FeedHub
// using backend.Models; // Required for Poll, PoliticianTwitterId etc.
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.SignalR; // Required for IHubContext
// using Microsoft.EntityFrameworkCore;
// using NSubstitute;
// using NUnit.Framework;

// namespace Tests.Controllers
// {
//     [TestFixture]
//     public class PollControllerAdminTests
//     {
//         private DataContext _context;
//         private PollsController _uut;
//         private IHubContext<FeedHub> _mockHubContext;
//         private IClientProxy _mockClientProxy; // Added for verifying SignalR calls

//         [SetUp]
//         public void Setup()
//         {
//             var options = new DbContextOptionsBuilder<DataContext>()
//                 .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                 .Options;
//             _context = new DataContext(options);
//             _context.Database.EnsureCreated();

//             _mockHubContext = Substitute.For<IHubContext<FeedHub>>();
//             var mockClients = Substitute.For<IHubClients>();
//             _mockClientProxy = Substitute.For<IClientProxy>(); // Initialize the field
//             _mockHubContext.Clients.Returns(mockClients);
//             mockClients.All.Returns(_mockClientProxy); // Use the field

//             _uut = new PollsController(_context, _mockHubContext);
//         }

//         [TearDown]
//         public void Teardown()
//         {
//             _context.Database.EnsureDeleted();
//             _context.Dispose();
//         }

//         private async Task<PoliticianTwitterId> SeedPolitician(
//             int id = 1,
//             string name = "Test Politician",
//             string handle = "testpol"
//         )
//         {
//             var politician = new PoliticianTwitterId
//             {
//                 Id = id,
//                 Name = name,
//                 TwitterHandle = handle,
//             };
//             _context.PoliticianTwitterIds.Add(politician);
//             await _context.SaveChangesAsync();
//             return politician;
//         }

//         private async Task<Poll> SeedPoll( // Helper method to seed a poll
//             int? pollId = null, // Allow nullable for DB generation
//             string question = "Test Poll",
//             int politicianId = 1,
//             List<string>? optionTexts = null, // Corrected to nullable list
//             DateTime? endedAt = null,
//             bool generateId = false // If true, we don't set Id and let DB generate
//         )
//         {
//             if (optionTexts == null)
//             {
//                 optionTexts = new List<string> { "Opt A", "Opt B" };
//             }

//             var poll = new Poll
//             {
//                 Question = question,
//                 PoliticianTwitterId = politicianId,
//                 CreatedAt = DateTime.UtcNow,
//                 EndedAt = endedAt,
//                 Options = optionTexts.Select(ot => new PollOption { OptionText = ot }).ToList(),
//             };

//             if (pollId.HasValue && !generateId)
//             {
//                 poll.Id = pollId.Value;
//             }

//             _context.Polls.Add(poll);
//             await _context.SaveChangesAsync();
//             return poll;
//         }

//         #region CreatePoll Tests

//         [Test]
//         public async Task CreatePoll_ValidDto_ReturnsCreatedAtActionResult()
//         {
//             // Arrange
//             var politician = await SeedPolitician(101);
//             var endedAtDate = DateTime.UtcNow.AddDays(7);
//             var createPollDto = new CreatePollDto
//             {
//                 Question = "Favorite color?",
//                 Options = new List<string> { "Red", "Blue" },
//                 PoliticianTwitterId = politician.Id,
//                 EndedAt = endedAtDate,
//             };

//             // Act
//             var result = await _uut.CreatePoll(createPollDto);

//             // Assert
//             Assert.That(result, Is.TypeOf<ActionResult<PollDetailsDto>>());
//             var actionResult = result.Result as CreatedAtActionResult;
//             Assert.That(actionResult, Is.Not.Null);
//             Assert.That(actionResult.StatusCode, Is.EqualTo(201));
//             var pollDetailsDto = actionResult.Value as PollDetailsDto;
//             Assert.That(pollDetailsDto, Is.Not.Null);
//             Assert.That(pollDetailsDto.Question, Is.EqualTo(createPollDto.Question));
//             Assert.That(pollDetailsDto.Options.Count, Is.EqualTo(2));
//             Assert.That(pollDetailsDto.EndedAt.HasValue, Is.True);
//             Assert.That(pollDetailsDto.EndedAt.Value.Date, Is.EqualTo(endedAtDate.Date));

//             var dbPoll = await _context
//                 .Polls.Include(p => p.Options)
//                 .FirstOrDefaultAsync(p => p.Id == pollDetailsDto.Id);
//             Assert.That(dbPoll, Is.Not.Null);
//             Assert.That(dbPoll.Question, Is.EqualTo(createPollDto.Question));
//             Assert.That(dbPoll.Options.Count, Is.EqualTo(2));
//             Assert.That(dbPoll.EndedAt.HasValue, Is.True);
//             // Compare dates and allow for minor precision differences in time due to ToUniversalTime()
//             Assert.That(
//                 Math.Abs((dbPoll.EndedAt.Value - endedAtDate.ToUniversalTime()).TotalSeconds),
//                 Is.LessThan(1)
//             );
//         }

//         [Test]
//         public async Task CreatePoll_PoliticianNotFound_ReturnsValidationProblem()
//         {
//             // Arrange
//             var createPollDto = new CreatePollDto
//             {
//                 Question = "Test Question",
//                 Options = new List<string> { "Opt1", "Opt2" },
//                 PoliticianTwitterId = 999, // Non-existent
//             };

//             // Act
//             var result = await _uut.CreatePoll(createPollDto);

//             // Assert
//             Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
//             var objectResult = result.Result as ObjectResult;
//             Assert.That(objectResult, Is.Not.Null);

//             Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
//             var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
//             Assert.That(validationProblemDetails, Is.Not.Null);

//             Assert.That(
//                 validationProblemDetails.Errors.ContainsKey(
//                     nameof(CreatePollDto.PoliticianTwitterId)
//                 ),
//                 Is.True
//             );
//         }

//         [Test]
//         public async Task CreatePoll_EmptyOption_ReturnsValidationProblem()
//         {
//             // Arrange
//             var politician = await SeedPolitician(102);
//             var createPollDto = new CreatePollDto
//             {
//                 Question = "Test Question",
//                 Options = new List<string> { "Opt1", "" }, // Empty option
//                 PoliticianTwitterId = politician.Id,
//             };

//             // Act
//             var result = await _uut.CreatePoll(createPollDto);

//             // Assert
//             Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
//             var objectResult = result.Result as ObjectResult;
//             Assert.That(objectResult, Is.Not.Null);

//             Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
//             var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
//             Assert.That(validationProblemDetails, Is.Not.Null);

//             Assert.That(
//                 validationProblemDetails.Errors.ContainsKey(nameof(CreatePollDto.Options)),
//                 Is.True
//             );
//         }

//         [Test]
//         public async Task CreatePoll_DuplicateOptions_ReturnsValidationProblem()
//         {
//             // Arrange
//             var politician = await SeedPolitician(103);
//             var createPollDto = new CreatePollDto
//             {
//                 Question = "Test Question",
//                 Options = new List<string> { "Opt1", "Opt1" }, // Duplicate options
//                 PoliticianTwitterId = politician.Id,
//             };

//             // Act
//             var result = await _uut.CreatePoll(createPollDto);

//             // Assert
//             Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
//             var objectResult = result.Result as ObjectResult;
//             Assert.That(objectResult, Is.Not.Null);

//             Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
//             var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
//             Assert.That(validationProblemDetails, Is.Not.Null);

//             Assert.That(
//                 validationProblemDetails.Errors.ContainsKey(nameof(CreatePollDto.Options)),
//                 Is.True
//             );
//         }

//         #endregion

//         #region UpdatePoll Tests

//         [Test]
//         public async Task UpdatePoll_ValidDto_ReturnsNoContentResult()
//         {
//             // Arrange
//             var politician1 = await SeedPolitician(201, "Pol1", "pol1");
//             var politician2 = await SeedPolitician(202, "Pol2", "pol2");

//             var initialPoll = new Poll
//             {
//                 Question = "Initial Question",
//                 PoliticianTwitterId = politician1.Id,
//                 Options = new List<PollOption> { new PollOption { OptionText = "OldOpt1" } },
//             };
//             _context.Polls.Add(initialPoll);
//             await _context.SaveChangesAsync();
//             _context.Entry(initialPoll).State = EntityState.Detached; // Detach to avoid tracking issues

//             var updateDto = new UpdatePollDto
//             {
//                 Question = "Updated Question",
//                 Options = new List<string> { "NewOpt1", "NewOpt2" },
//                 PoliticianTwitterId = politician2.Id,
//                 EndedAt = DateTime.UtcNow.AddDays(10),
//             };

//             // Act
//             var result = await _uut.UpdatePoll(initialPoll.Id, updateDto);

//             // Assert
//             Assert.That(result, Is.TypeOf<NoContentResult>());

//             var updatedDbPoll = await _context
//                 .Polls.Include(p => p.Options)
//                 .FirstOrDefaultAsync(p => p.Id == initialPoll.Id);
//             Assert.That(updatedDbPoll, Is.Not.Null);
//             Assert.That(updatedDbPoll!.Question, Is.EqualTo(updateDto.Question));
//             Assert.That(updatedDbPoll.PoliticianTwitterId, Is.EqualTo(politician2.Id));
//             Assert.That(updatedDbPoll.Options.Count, Is.EqualTo(2));
//             Assert.That(updatedDbPoll.Options.Any(o => o.OptionText == "NewOpt1"), Is.True);
//             Assert.That(
//                 updatedDbPoll.EndedAt!.Value.Date,
//                 Is.EqualTo(updateDto.EndedAt!.Value.Date)
//             );
//         }

//         [Test]
//         public async Task UpdatePoll_PollNotFound_ReturnsNotFoundResult()
//         {
//             // Arrange
//             var updateDto = new UpdatePollDto
//             {
//                 Question = "Q",
//                 Options = new List<string> { "O" },
//                 PoliticianTwitterId = 1,
//             };

//             // Act
//             var result = await _uut.UpdatePoll(999, updateDto); // Non-existent poll ID

//             // Assert
//             Assert.That(result, Is.TypeOf<NotFoundResult>());
//         }

//         [Test]
//         public async Task UpdatePoll_PoliticianNotFound_ReturnsValidationProblem()
//         {
//             // Arrange
//             var politician = await SeedPolitician(203);
//             var poll = new Poll
//             {
//                 Question = "Q",
//                 PoliticianTwitterId = politician.Id,
//                 Options = new List<PollOption>(),
//             };
//             _context.Polls.Add(poll);
//             await _context.SaveChangesAsync();

//             var updateDto = new UpdatePollDto
//             {
//                 Question = "Updated Q",
//                 Options = new List<string> { "Opt1" },
//                 PoliticianTwitterId = 998, // Non-existent politician
//             };

//             // Act
//             var result = await _uut.UpdatePoll(poll.Id, updateDto);

//             // Assert
//             Assert.That(result, Is.InstanceOf<ObjectResult>());
//             var objectResult = result as ObjectResult;
//             Assert.That(objectResult, Is.Not.Null);

//             Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
//             var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
//             Assert.That(validationProblemDetails, Is.Not.Null);

//             Assert.That(
//                 validationProblemDetails.Errors.ContainsKey(
//                     nameof(UpdatePollDto.PoliticianTwitterId)
//                 ),
//                 Is.True
//             );
//         }

//         [Test]
//         public async Task UpdatePoll_EmptyOption_ReturnsValidationProblem()
//         {
//             // Arrange
//             var politician = await SeedPolitician(204);
//             var poll = new Poll
//             {
//                 Question = "Q",
//                 PoliticianTwitterId = politician.Id,
//                 Options = new List<PollOption>(),
//             };
//             _context.Polls.Add(poll);
//             await _context.SaveChangesAsync();

//             var updateDto = new UpdatePollDto
//             {
//                 Question = "Updated Q",
//                 Options = new List<string> { "Opt1", "" }, // Empty option
//                 PoliticianTwitterId = politician.Id,
//             };

//             // Act
//             var result = await _uut.UpdatePoll(poll.Id, updateDto);

//             // Assert
//             Assert.That(result, Is.InstanceOf<ObjectResult>());
//             var objectResult = result as ObjectResult;
//             Assert.That(objectResult, Is.Not.Null);

//             Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
//             var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
//             Assert.That(validationProblemDetails, Is.Not.Null);

//             Assert.That(
//                 validationProblemDetails.Errors.ContainsKey(nameof(UpdatePollDto.Options)),
//                 Is.True
//             );
//         }

//         [Test]
//         public async Task UpdatePoll_DuplicateOptions_ReturnsValidationProblem()
//         {
//             // Arrange
//             var politician = await SeedPolitician(205);
//             var poll = new Poll
//             {
//                 Question = "Q",
//                 PoliticianTwitterId = politician.Id,
//                 Options = new List<PollOption>(),
//             };
//             _context.Polls.Add(poll);
//             await _context.SaveChangesAsync();

//             var updateDto = new UpdatePollDto
//             {
//                 Question = "Updated Q",
//                 Options = new List<string> { "Opt1", "Opt1" }, // Duplicate options
//                 PoliticianTwitterId = politician.Id,
//             };

//             // Act
//             var result = await _uut.UpdatePoll(poll.Id, updateDto);

//             // Assert
//             Assert.That(result, Is.InstanceOf<ObjectResult>());
//             var objectResult = result as ObjectResult;
//             Assert.That(objectResult, Is.Not.Null);

//             Assert.That(objectResult.Value, Is.InstanceOf<ValidationProblemDetails>());
//             var validationProblemDetails = objectResult.Value as ValidationProblemDetails;
//             Assert.That(validationProblemDetails, Is.Not.Null);

//             Assert.That(
//                 validationProblemDetails.Errors.ContainsKey(nameof(UpdatePollDto.Options)),
//                 Is.True
//             );
//         }

//         #endregion

//         #region DeletePoll Tests

//         [Test]
//         public async Task DeletePoll_PollNotFound_ReturnsNotFoundResult()
//         {
//             // Arrange
//             var nonExistentPollId = 999;

//             // Act
//             var result = await _uut.DeletePoll(nonExistentPollId);

//             // Assert
//             Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
//             var notFoundResult = result as NotFoundObjectResult;
//             Assert.That(notFoundResult, Is.Not.Null, "NotFoundResult should not be null.");
//             Assert.That(
//                 notFoundResult!.Value,
//                 Is.EqualTo($"Poll med ID {nonExistentPollId} blev ikke fundet.")
//             );
//         }

//         [Test]
//         public async Task DeletePoll_ExistingPoll_DeletesPollAndRelatedData_ReturnsOk_NotifiesHub()
//         {
//             // Arrange
//             var politician = await SeedPolitician(
//                 id: 301,
//                 name: "PolForDelete",
//                 handle: "polfordel"
//             );
//             var poll = await SeedPoll(
//                 politicianId: politician.Id,
//                 optionTexts: new List<string> { "DelOpt1", "DelOpt2" },
//                 generateId: true
//             );

//             // Seed some votes for the poll
//             var userVote1 = new UserVote
//             {
//                 PollId = poll.Id,
//                 ChosenOptionId = poll.Options[0].Id,
//                 UserId = 1,
//             };
//             var userVote2 = new UserVote
//             {
//                 PollId = poll.Id,
//                 ChosenOptionId = poll.Options[1].Id,
//                 UserId = 2,
//             };
//             _context.UserVotes.AddRange(userVote1, userVote2);
//             await _context.SaveChangesAsync();

//             var pollIdToDelete = poll.Id;
//             var optionIdsToDelete = poll.Options.Select(o => o.Id).ToList();

//             // Act
//             var result = await _uut.DeletePoll(pollIdToDelete);

//             // Assert
//             Assert.That(result, Is.TypeOf<OkObjectResult>());
//             var okResult = result as OkObjectResult;
//             Assert.That(okResult, Is.Not.Null, "OkResult should not be null.");
//             Assert.That(okResult!.Value, Is.Not.Null, "OkResult.Value should not be null.");

//             var messageProperty = okResult.Value.GetType().GetProperty("message");
//             Assert.That(
//                 messageProperty,
//                 Is.Not.Null,
//                 "Message property should exist on the OkResult value."
//             );
//             var messageValue = messageProperty!.GetValue(okResult.Value, null);
//             Assert.That(messageValue, Is.EqualTo($"Poll med ID {pollIdToDelete} blev slettet."));

//             // Verify data is deleted
//             var deletedPoll = await _context.Polls.FindAsync(pollIdToDelete);
//             Assert.That(deletedPoll, Is.Null, "Poll should be deleted.");

//             foreach (var optionId in optionIdsToDelete)
//             {
//                 var deletedOption = await _context.PollOptions.FindAsync(optionId);
//                 Assert.That(
//                     deletedOption,
//                     Is.Null,
//                     $"PollOption with Id {optionId} should be deleted."
//                 );
//             }

//             var remainingVotes = await _context
//                 .UserVotes.Where(uv => uv.PollId == pollIdToDelete)
//                 .ToListAsync();
//             Assert.That(remainingVotes, Is.Empty, "UserVotes for the poll should be deleted.");

//             // Verify SignalR Hub was called
//             await _mockClientProxy
//                 .Received(1)
//                 .SendCoreAsync(
//                     "PollDeleted",
//                     Arg.Is<object[]>(args => args.Length == 1 && (int)args[0] == pollIdToDelete)
//                 );
//         }

//         #endregion
//     }
// }
