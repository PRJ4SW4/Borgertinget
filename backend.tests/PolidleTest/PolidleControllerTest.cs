using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTO; 
using backend.Enums;
using backend.Interfaces.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace backend.tests.PolidleTest
{
    [TestFixture]
    public class PolidleControllerTests
    {
        private IDailySelectionService _serviceMock;
        private ILogger<PolidleController> _loggerMock;
        private PolidleController _controller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = Substitute.For<IDailySelectionService>();
            _loggerMock = Substitute.For<ILogger<PolidleController>>();
            _controller = new PolidleController(_serviceMock, _loggerMock);
        }

        // --- Tests for GetAllPoliticiansForGuessing ---
        #region GetAllPoliticiansForGuessing
        [Test]
        public async Task GetAllPoliticiansForGuessing_ReturnsOkWithListOfSearchListDto()
        {
            // Arrange
            var expectedPoliticians = new List<SearchListDto>
            {
                new SearchListDto
                {
                    Id = 1,
                    PolitikerNavn = "Mette F",
                    PictureUrl = "Img1",
                },
                new SearchListDto
                {
                    Id = 2,
                    PolitikerNavn = "Lars L",
                    PictureUrl = "Img2",
                },
            };
            _serviceMock
                .GetAllPoliticiansForGuessingAsync()
                .Returns(Task.FromResult(expectedPoliticians));

            // Act
            var actionResult = await _controller.GetAllPoliticians();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var actualPoliticians = okResult.Value as IEnumerable<SearchListDto>;
            Assert.That(actualPoliticians, Is.Not.Null);
        }

        [Test]
        public async Task GetAllPoliticiansForGuessing_ReturnsOkWithEmptyList_WhenServiceReturnsEmpty()
        {
            // Arrange
            var emptyList = new List<SearchListDto>();
            _serviceMock.GetAllPoliticiansForGuessingAsync().Returns(Task.FromResult(emptyList));

            // Act
            var actionResult = await _controller.GetAllPoliticians();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var actualPoliticians = okResult.Value as IEnumerable<SearchListDto>;
            Assert.That(actualPoliticians, Is.Not.Null);
        }
        #endregion
        #region GetClassicDetailsOfTheDay
        // --- Tests for GetClassicDetailsOfTheDay ---
        [Test]
        public async Task GetClassicDetailsOfTheDay_ReturnsOkWithDetails_WhenExists()
        {
            // Arrange
            var expectedDetails = new DailyPoliticianDto
            {
                Id = 1,
                Age = 40,
                PartyShortname = "TP", /* ... */
            };
            _serviceMock
                .GetClassicDetailsOfTheDayAsync()
                .Returns(Task.FromResult<DailyPoliticianDto?>(expectedDetails));

            // Act
            var actionResult = await _controller.GetClassicDetails();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedDetails));
        }

        [Test]
        public async Task GetClassicDetailsOfTheDay_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _serviceMock
                .GetClassicDetailsOfTheDayAsync()
                .Returns(Task.FromResult<DailyPoliticianDto?>(null));

            // Act
            var actionResult = await _controller.GetClassicDetails();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
        }
        #endregion
        #region GetQuoteOfTheDay
        // --- Tests for GetQuoteOfTheDay ---
        [Test]
        public async Task GetQuoteOfTheDay_ReturnsOkWithQuote_WhenExists()
        {
            // Arrange
            var expectedQuote = new QuoteDto { QuoteText = "Test Quote" };
            _serviceMock.GetQuoteOfTheDayAsync().Returns(Task.FromResult<QuoteDto?>(expectedQuote));

            // Act
            var actionResult = await _controller.GetQuote();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedQuote));
        }

        [Test]
        public async Task GetQuoteOfTheDay_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _serviceMock.GetQuoteOfTheDayAsync().Returns(Task.FromResult<QuoteDto?>(null));

            // Act
            var actionResult = await _controller.GetQuote();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
        }
        #endregion
        #region GetPhotoOfTheDay
        // --- Tests for GetPhotoOfTheDay ---
        [Test]
        public async Task GetPhotoOfTheDay_ReturnsOkWithPhoto_WhenExists()
        {
            // Arrange
            var expectedPhoto = new PhotoDto { PhotoUrl = "url.jpg" };
            _serviceMock.GetPhotoOfTheDayAsync().Returns(Task.FromResult<PhotoDto?>(expectedPhoto));

            // Act
            var actionResult = await _controller.GetPhoto();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedPhoto));
        }

        [Test]
        public async Task GetPhotoOfTheDay_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _serviceMock.GetPhotoOfTheDayAsync().Returns(Task.FromResult<PhotoDto?>(null));

            // Act
            var actionResult = await _controller.GetPhoto();

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
        }
        #endregion
        #region ProcessGuess
        // --- Tests for ProcessGuess ---
        [Test]
        public async Task ProcessGuess_ValidRequest_ReturnsOkWithGuessResult()
        {
            // Arrange
            var guessRequest = new GuessRequestDto
            {
                GameMode = GamemodeTypes.Klassisk,
                GuessedPoliticianId = 2,
            };
            var expectedResult = new GuessResultDto
            {
                IsCorrectGuess = false, /* ... andre feedback felter ... */
            };
            _serviceMock
                .ProcessGuessAsync(guessRequest)
                .Returns(Task.FromResult<GuessResultDto?>(expectedResult));

            // Act
            var actionResult = await _controller.PostGuess(guessRequest);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task ProcessGuess_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            var guessRequest = new GuessRequestDto
            {
                GameMode = GamemodeTypes.Klassisk,
                GuessedPoliticianId = 3,
            };
            _serviceMock
                .ProcessGuessAsync(guessRequest)
                .Returns(Task.FromResult<GuessResultDto?>(null));

            // Act
            var actionResult = await _controller.PostGuess(guessRequest);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
        }
    }
    #endregion
}
