using backend.Controllers;
using backend.Data;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Controllers
{
    [TestFixture]
    public class AdministratorControllerTests
    {
        private AdministratorController _uut;
        private IAdministratorService _mockService;
        private ILogger<AdministratorController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockService = Substitute.For<IAdministratorService>();
            _mockLogger = Substitute.For<ILogger<AdministratorController>>();
            _uut = new AdministratorController(_mockService, _mockLogger);
        }

        #region Flashcard Collection POST

        [Test]
        public async Task PostFlashCardCollection_ValidDto_ReturnsOk()
        {
            var dto = new FlashcardCollectionDetailDTO { Title = "Test" };
            _mockService.CreateCollectionAsync(dto).Returns(42);

            var result = await _uut.PostFlashCardCollection(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo("Flashcard Collection created with ID 42"));
        }

        [Test]
        public async Task PostFlashCardCollection_NullDto_ReturnsBadRequest()
        {
            var result = await _uut.PostFlashCardCollection(null!);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest?.Value, Is.EqualTo("No Collection to create from"));
        }

        [Test]
        public async Task PostFlashCardCollection_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var dto = new FlashcardCollectionDetailDTO { Title = "Test" };
            _mockService.CreateCollectionAsync(dto).Throws(new Exception("Unexpected error"));

            // Act
            var result = await _uut.PostFlashCardCollection(dto);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value,
                Does.Contain("An error occurred while creating the collection: Unexpected error")
            );
        }

        [Test]
        public async Task UploadImage_ValidFile_ReturnsImagePath()
        {
            // Mock file upload
            var fileMock = Substitute.For<IFormFile>();
            fileMock.Length.Returns(1024);
            fileMock.FileName.Returns("andersfogh.png");

            var stream = new MemoryStream();
            fileMock.OpenReadStream().Returns(stream);
            fileMock
                .CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            // Call controller method
            var result = await _uut.UploadImage(fileMock);

            // Assert expected result
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value?.ToString(), Does.Contain("/uploads/flashcards/andersfogh.png"));
        }

        [Test]
        public async Task UploadImage_NullFile_ReturnsBadRequest()
        {
            var result = await _uut.UploadImage(null!);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("No file uploaded"));
        }

        [Test]
        public async Task UploadImage_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var fileMock = Substitute.For<IFormFile>();
            fileMock.Length.Returns(1024);
            fileMock.FileName.Returns("andersfogh.png");

            // Simulate exception during file upload
            fileMock
                .When(f => f.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()))
                .Do(x => throw new Exception("Unexpected error"));

            // Act
            var result = await _uut.UploadImage(fileMock);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value,
                Does.Contain("An error occurred while uploading the file: Unexpected error")
            );
        }

        #endregion


        #region Flashcard Collection GET

        [Test]
        public async Task GetAllFlashcardCollectionTitles_ReturnsOkWithList()
        {
            // Arrange list of titles
            var titles = new List<string> { "Politikerer", "Partier" };
            _mockService.GetAllFlashcardCollectionTitlesAsync().Returns(titles);

            // Act on the service call
            var result = await _uut.GetFlashCardCollectionTitles();

            // Assert the right return statement
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(titles));
        }

        [Test]
        public async Task GetAllFlashcardCollectionTitles_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockService
                .GetAllFlashcardCollectionTitlesAsync()
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = await _uut.GetFlashCardCollectionTitles();

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value?.ToString(),
                Is.EqualTo("Error Fetching Flashcard Collection titles: Unexpected error")
            );
        }

        [Test]
        public async Task GetFlashcardCollectionByTitle_ReturnsMatchingDto()
        {
            // Arrange
            var dto = new FlashcardCollectionDetailDTO { Title = "Blå partier" };
            _mockService.GetFlashCardCollectionByTitle("Blå partier").Returns(dto);

            // Act
            var result = await _uut.GetFlashCardCollectionByTitle("Blå partier");

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(dto));
        }

        [Test]
        public async Task GetFlashcardCollectionByTitle_ServiceThrows_ReturnsInternalServerError()
        {
            // Arrange
            _mockService
                .GetFlashCardCollectionByTitle("Statsministerer")
                .Throws(new Exception("Something went wrong"));

            // Act
            var result = await _uut.GetFlashCardCollectionByTitle("Statsministerer");

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value?.ToString(),
                Is.EqualTo("Error finding Flashcard Collection by title: Something went wrong")
            );
        }

        #endregion

        #region Flashcard Collection DELETE

        [Test]
        public async Task DeleteFlashcardCollection_ValidId_ReturnsSuccessMessage()
        {
            // Act
            var result = await _uut.DeleteFlashcardCollection(10);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Flashcard collection with ID 10 deleted"));
        }

        [Test]
        public async Task DeleteFlashcardCollection_InvalidId_ReturnsBadRequest()
        {
            // Act
            var result = await _uut.DeleteFlashcardCollection(0);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("Enter a valid ID"));
        }

        [Test]
        public async Task DeleteFlashcardCollection_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockService
                .DeleteFlashcardCollectionAsync(10)
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _uut.DeleteFlashcardCollection(10);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value,
                Does.Contain(
                    "An error occurred while deleting the Flashcard collection: Unexpected error"
                )
            );
        }

        #endregion


        #region Flashcard Collection PUT

        [Test]
        public async Task UpdateFlashcardCollection_ValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "Opdateret samling",
                Flashcards = new List<FlashcardDTO>(),
            };

            // Act
            var result = await _uut.UpdateFlashcardCollection(1, dto);

            await _mockService.Received(1).UpdateCollectionInfoAsync(1, dto);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Flashcard collection updated successfully"));
        }

        [Test]
        public async Task UpdateFlashcardCollection_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _uut.UpdateFlashcardCollection(1, null!);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("No collection data provided"));
        }

        [Test]
        public async Task UpdateFlashcardCollection_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "Test Collection",
                Flashcards = new List<FlashcardDTO>(),
            };

            _mockService
                .UpdateCollectionInfoAsync(1, dto)
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _uut.UpdateFlashcardCollection(1, dto);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value,
                Does.Contain("An error occurred while updating the collection: Unexpected error")
            );
        }

        #endregion

        #region Username GET

        [Test]
        public async Task GetUsernameID_ValidUsername_ReturnsUserId()
        {
            // Arrange
            var userId = new UserIdDTO { UserId = 88 };
            _mockService.GetUserIdByUsernameAsync("hellethorning").Returns(userId);

            // Act
            var result = await _uut.GetUsernameID("hellethorning");

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(88));
        }

        [Test]
        public async Task GetUsernameID_NullUsername_ReturnsBadRequest()
        {
            // Act
            var result = await _uut.GetUsernameID(null!);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("Enter valid username"));
        }

        [Test]
        public async Task GetUsernameID_ServiceFails_ReturnsInternalServerError()
        {
            // Arrange
            _mockService
                .GetUserIdByUsernameAsync("invaliduser")
                .Throws(new Exception("Database error"));

            // Act
            var result = await _uut.GetUsernameID("invaliduser");

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        #endregion

        #region Username PUT

        [Test]
        public async Task PutNewUserName_ValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new UpdateUserNameDto { UserName = "larslykke" };

            // Act
            var result = await _uut.PutNewUserName(5, dto);

            await _mockService.Received(1).UpdateUserNameAsync(5, dto);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Username updated"));
        }

        [Test]
        public async Task PutNewUserName_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _uut.PutNewUserName(5, null!);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("No new username found"));
        }

        [Test]
        public async Task PutNewUserName_ServiceFails_ReturnsInternalServerError()
        {
            // Arrange
            var dto = new UpdateUserNameDto { UserName = "newusername" };
            _mockService.UpdateUserNameAsync(5, dto).Throws(new Exception("Update failed"));

            // Act
            var result = await _uut.PutNewUserName(5, dto);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        #endregion

        #region Citat-mode GET

        [Test]
        public async Task GetAllQuotes_ReturnsListOfQuotes()
        {
            // Arrange
            var quotes = new List<EditQuoteDTO>
            {
                new EditQuoteDTO { QuoteId = 1, QuoteText = "Statsministeren siger ja" },
            };
            _mockService.GetAllQuotesAsync().Returns(quotes);

            // Act
            var result = await _uut.GetAllQuotes();

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(quotes));
        }

        [Test]
        public async Task GetAllQuotes_ServiceFails_Returns500()
        {
            // Arrange
            _mockService.GetAllQuotesAsync().Throws(new Exception("db unavailable"));

            // Act
            var result = await _uut.GetAllQuotes();

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
            Assert.That(obj?.Value?.ToString(), Does.Contain("An error occurred"));
        }

        [Test]
        public async Task GetQuoteById_ValidId_ReturnsQuote()
        {
            // Arrange
            var quote = new EditQuoteDTO
            {
                QuoteId = 2,
                QuoteText = "Vi lukker ikke flere minkfarme",
            };
            _mockService.GetQuoteByIdAsync(2).Returns(quote);

            // Act
            var result = await _uut.GetQuoteById(2);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(quote));
        }

        [Test]
        public async Task GetQuoteById_ServiceFails_Returns500()
        {
            // Arrange
            var quoteId = 99;
            _mockService.GetQuoteByIdAsync(quoteId).Throws(new Exception("not found"));

            // Act
            var result = await _uut.GetQuoteById(quoteId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
            Assert.That(
                obj?.Value?.ToString(),
                Is.EqualTo("An error occurred while getting quote: not found")
            );
        }

        #endregion

        #region EditQuote PUT

        [Test]
        public async Task EditQuote_ValidInput_ReturnsOk()
        {
            var dto = new EditQuoteDTO { QuoteId = 3, QuoteText = "Vi må stå sammen" };
            var result = await _uut.EditQuote(dto);

            await _mockService.Received(1).EditQuoteAsync(3, "Vi må stå sammen");

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Quote edited"));
        }

        [Test]
        public async Task EditQuote_ServiceFails_Returns500()
        {
            // Arrange
            _mockService.EditQuoteAsync(4, "Fejl i citat").Throws(new Exception("database error"));

            var dto = new EditQuoteDTO { QuoteId = 4, QuoteText = "Fejl i citat" };

            // Act
            var result = await _uut.EditQuote(dto);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value?.ToString(),
                Is.EqualTo("An error occurred while editing the quote: database error")
            );
        }

        #endregion

        #region GetAktorIdByTwitterId

        [Test]
        public async Task GetAktorIdByTwitterId_ValidId_ReturnsOkWithAktorId()
        {
            // Arrange
            var twitterId = 123;
            var expectedAktorId = 456;
            _mockService.GetAktorIdByTwitterIdAsync(twitterId).Returns(expectedAktorId);

            // Act
            var result = await _uut.GetAktorIdByTwitterId(twitterId);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            dynamic value = okResult.Value!;
            Assert.That(
                value.GetType().GetProperty("aktorId")?.GetValue(value, null),
                Is.EqualTo(expectedAktorId)
            );
        }

        [Test]
        public async Task GetAktorIdByTwitterId_NotFound_ReturnsNotFound()
        {
            // Arrange
            var twitterId = 789;
            _mockService.GetAktorIdByTwitterIdAsync(twitterId).Returns((int?)null);

            // Act
            var result = await _uut.GetAktorIdByTwitterId(twitterId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(
                notFoundResult?.Value,
                Is.EqualTo($"No AktorId found for Twitter ID: {twitterId}")
            );
        }

        [Test]
        public async Task GetAktorIdByTwitterId_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidTwitterId = -1;

            // Act
            var result = await _uut.GetAktorIdByTwitterId(invalidTwitterId);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid Twitter ID."));
        }

        [Test]
        public async Task GetAktorIdByTwitterId_ServiceFails_ReturnsInternalServerError()
        {
            // Arrange
            var twitterId = 123;
            _mockService
                .GetAktorIdByTwitterIdAsync(twitterId)
                .Throws(new Exception("Lookup failed"));

            // Act
            var result = await _uut.GetAktorIdByTwitterId(twitterId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(
                objectResult.Value?.ToString(),
                Is.EqualTo("An error occurred while looking up AktorId: Lookup failed")
            );
        }

        #endregion
    }
}
