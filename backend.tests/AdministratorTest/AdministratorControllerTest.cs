using backend.Data;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Controllers
{
    [TestFixture]
    public class AdministratorControllerTests
    {
        private AdministratorController _controller;
        private IAdministratorService _service;
        private DataContext _context;

        [SetUp]
        public void Setup()
        {
            _service = Substitute.For<IAdministratorService>();
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new DataContext(options);
            _controller = new AdministratorController(_service, _context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Database.EnsureDeleted(); // clean after each test
            _context.Dispose();
        }

        #region Flashcard Collection POST

        [Test]
        public async Task PostFlashCardCollection_ValidDto_ReturnsOk()
        {
            var dto = new FlashcardCollectionDetailDTO { Title = "Test" };
            _service.CreateCollectionAsync(dto).Returns(42);

            var result = await _controller.PostFlashCardCollection(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo("Flashcard Collection created with ID 42"));
        }

        [Test]
        public async Task PostFlashCardCollection_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.PostFlashCardCollection(null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest?.Value, Is.EqualTo("No Collection to create from"));
        }

        [Test]
        public async Task UploadImage_ValidFile_ReturnsImagePath()
        {
            var fileMock = Substitute.For<IFormFile>();
            fileMock.Length.Returns(1024);
            fileMock.FileName.Returns("andersfogh.png");

            var stream = new MemoryStream();
            fileMock.OpenReadStream().Returns(stream);
            fileMock
                .CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var result = await _controller.UploadImage(fileMock);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value?.ToString(), Does.Contain("/uploads/flashcards/andersfogh.png"));
        }

        [Test]
        public async Task UploadImage_NullFile_ReturnsBadRequest()
        {
            var result = await _controller.UploadImage(null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("No file uploaded"));
        }

        #endregion


        #region Flashcard Collection GET

        [Test]
        public async Task GetAllFlashcardCollectionTitles_ReturnsOkWithList()
        {
            var titles = new List<string> { "Politikerer", "Partier" };
            _service.GetAllFlashcardCollectionTitlesAsync().Returns(titles);

            var result = await _controller.GetFlashCardCollectionTitles();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(titles));
        }

        [Test]
        public async Task GetFlashcardCollectionByTitle_ReturnsMatchingDto()
        {
            var dto = new FlashcardCollectionDetailDTO { Title = "Blå partier" };
            _service.GetFlashCardCollectionByTitle("Blå partier").Returns(dto);

            var result = await _controller.GetFlashCardCollectionByTitle("Blå partier");

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(dto));
        }

        [Test]
        public async Task GetFlashcardCollectionByTitle_ServiceThrows_ReturnsInternalServerError()
        {
            _service
                .GetFlashCardCollectionByTitle("Statsministerer")
                .Throws(new System.Exception("Something went wrong"));

            var result = await _controller.GetFlashCardCollectionByTitle("Statsministerer");

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult?.Value?.ToString(), Does.Contain("Something went wrong"));
        }

        #endregion

        #region Flashcard Collection DELETE

        [Test]
        public async Task DeleteFlashcardCollection_ValidId_ReturnsSuccessMessage()
        {
            var result = await _controller.DeleteFlashcardCollection(10);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Flashcard collection with ID 10 deleted"));
        }

        [Test]
        public async Task DeleteFlashcardCollection_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteFlashcardCollection(0);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("Enter a valid ID"));
        }

        #endregion


        #region Flashcard Collection PUT

        [Test]
        public async Task UpdateFlashcardCollection_ValidDto_ReturnsOk()
        {
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "Opdateret samling",
                Flashcards = new List<FlashcardDTO>(),
            };

            var result = await _controller.UpdateFlashcardCollection(1, dto);

            await _service.Received(1).UpdateCollectionInfoAsync(1, dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Flashcard collection updated successfully"));
        }

        [Test]
        public async Task UpdateFlashcardCollection_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.UpdateFlashcardCollection(1, null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("No collection data provided"));
        }

        #endregion

        #region Username GET

        [Test]
        public async Task GetAllUsers_ReturnsOkWithUsers()
        {
            var users = new[]
            {
                new User { Id = 1, UserName = "hummelgaard" },
            };
            _service.GetAllUsersAsync().Returns(users);

            var result = await _controller.GetAllUsers();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(users));
        }

        [Test]
        public async Task GetAllUsers_ServiceThrows_Returns500()
        {
            _service.GetAllUsersAsync().Throws(new Exception("db error"));

            var result = await _controller.GetAllUsers();

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult?.Value?.ToString(), Does.Contain("db error"));
        }

        [Test]
        public async Task GetUsernameID_ValidUsername_ReturnsUserId()
        {
            var user = new User { Id = 88, UserName = "hellethorning" };
            _service.GetUserByUsernameAsync("hellethorning").Returns(user);

            var result = await _controller.GetUsernameID("hellethorning");

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(88));
        }

        [Test]
        public async Task GetUsernameID_NullUsername_ReturnsBadRequest()
        {
            var result = await _controller.GetUsernameID(null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("Enter valid username"));
        }

        #endregion

        #region Username PUT

        [Test]
        public async Task PutNewUserName_ValidDto_ReturnsOk()
        {
            var dto = new UpdateUserNameDto { UserName = "larslykke" };

            var result = await _controller.PutNewUserName(5, dto);

            await _service.Received(1).UpdateUserNameAsync(5, dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Username updated"));
        }

        [Test]
        public async Task PutNewUserName_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.PutNewUserName(5, null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("No new username found"));
        }

        #endregion

        #region Citat-mode GET

        [Test]
        public async Task GetAllQuotes_ReturnsListOfQuotes()
        {
            var quotes = new List<EditQuoteDTO>
            {
                new EditQuoteDTO { QuoteId = 1, QuoteText = "Statsministeren siger ja" },
            };
            _service.GetAllQuotesAsync().Returns(quotes);

            var result = await _controller.GetAllQuotes();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(quotes));
        }

        [Test]
        public async Task GetAllQuotes_ServiceFails_Returns500()
        {
            _service.GetAllQuotesAsync().Throws(new Exception("db unavailable"));

            var result = await _controller.GetAllQuotes();

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
            Assert.That(obj?.Value?.ToString(), Does.Contain("An error occured"));
        }

        [Test]
        public async Task GetQuoteById_ValidId_ReturnsQuote()
        {
            var quote = new EditQuoteDTO
            {
                QuoteId = 2,
                QuoteText = "Vi lukker ikke flere minkfarme",
            };
            _service.GetQuoteByIdAsync(2).Returns(quote);

            var result = await _controller.GetQuoteById(2);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(quote));
        }

        [Test]
        public async Task GetQuoteById_ServiceFails_Returns500()
        {
            _service.GetQuoteByIdAsync(99).Throws(new Exception("not found"));

            var result = await _controller.GetQuoteById(99);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
            Assert.That(obj?.Value?.ToString(), Does.Contain("99"));
        }

        #endregion

        #region EditQuote PUT

        [Test]
        public async Task EditQuote_ValidInput_ReturnsOk()
        {
            var result = await _controller.EditQuote(3, "Vi må stå sammen");

            await _service.Received(1).EditQuoteAsync(3, "Vi må stå sammen");

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Quote edited"));
        }

        [Test]
        public async Task EditQuote_ServiceFails_Returns500()
        {
            _service.EditQuoteAsync(4, "Fejl i citat").Throws(new Exception("database error"));

            var result = await _controller.EditQuote(4, "Fejl i citat");

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
            Assert.That(obj?.Value?.ToString(), Does.Contain("4"));
        }

        #endregion
    }
}
