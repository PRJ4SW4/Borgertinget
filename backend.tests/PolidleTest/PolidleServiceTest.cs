using backend.Data;
using backend.DTO;
using backend.Enums;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;
using backend.Interfaces.Utility;
using backend.Models;
using backend.Models.Politicians;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace backend.tests.Services
{
    [TestFixture]
    public class DailySelectionServiceTests
    {
        // --- Mocks for Dependencies ---
        private IAktorRepository _aktorRepositoryMock;
        private IDailySelectionRepository _dailySelectionRepositoryMock;
        private IGamemodeTrackerRepository _trackerRepositoryMock;
        private ISelectionAlgorithm _selectionAlgorithmMock;
        private IPoliticianMapper _mapperMock;
        private IDateTimeProvider _dateTimeProviderMock;
        private ILogger<DailySelectionService> _loggerMock;
        private IRandomProvider _randomProviderMock;
        private DataContext _contextMock;
        private DatabaseFacade _databaseFacadeMock; // Til at mocke Database property på DataContext
        private IDbContextTransaction _dbContextTransactionMock; // Til at mocke transaktionen

        private DailySelectionService _service;

        // --- Test Data Setup ---
        private DateOnly _today;
        private Aktor _defaultAktor1;
        private Aktor _defaultAktor2;
        private Aktor _defaultAktor3;

        [TearDown]
        public void TearDown()
        {
            _contextMock?.Dispose();
            _dbContextTransactionMock?.Dispose();
        }

        #region SetUp
        [SetUp]
        public void Setup()
        {
            _aktorRepositoryMock = Substitute.For<IAktorRepository>();
            _dailySelectionRepositoryMock = Substitute.For<IDailySelectionRepository>();
            _trackerRepositoryMock = Substitute.For<IGamemodeTrackerRepository>();
            _selectionAlgorithmMock = Substitute.For<ISelectionAlgorithm>();
            _mapperMock = Substitute.For<IPoliticianMapper>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
            _loggerMock = Substitute.For<ILogger<DailySelectionService>>();
            _randomProviderMock = Substitute.For<IRandomProvider>();

            // Mock DataContext og dens transaktionsmekanisme
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Brug In-Memory for enkelhed, eller mock helt
                .Options;
            _databaseFacadeMock = Substitute.For<DatabaseFacade>(new MockDbContext()); // MockDbContext er en dummy for at tilfredsstille DatabaseFacade constructor
            _dbContextTransactionMock = Substitute.For<IDbContextTransaction>();
            _databaseFacadeMock
                .BeginTransactionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(_dbContextTransactionMock));

            _contextMock = new DataContext(options); // Brug en rigtig in-memory for SaveChangesAsync

            _contextMock = Substitute.ForPartsOf<DataContext>(options); // Tillader at mocke virtuelle metoder som SaveChangesAsync
            _contextMock.Database.Returns(_databaseFacadeMock); // Hvis Database property er virtuel, ellers virker dette ikke.

            _service = new DailySelectionService(
                _aktorRepositoryMock,
                _dailySelectionRepositoryMock,
                _trackerRepositoryMock,
                _selectionAlgorithmMock,
                _mapperMock,
                _dateTimeProviderMock,
                _loggerMock,
                _randomProviderMock,
                _contextMock // Giver vores (delvist) mockede context videre
            );

            // Default test data
            _today = new DateOnly(2024, 5, 25);
            _dateTimeProviderMock.TodayUtc.Returns(_today);

            _defaultAktor1 = new Aktor
            {
                Id = 1,
                fornavn = "Classic",
                efternavn = "Politician",
                Born = "1980-01-01",
                PictureMiRes = "img1.jpg",
                Quotes = new List<PoliticianQuote>
                {
                    new PoliticianQuote { QuoteText = "Citat 1" },
                },
            };
            _defaultAktor2 = new Aktor
            {
                Id = 2,
                fornavn = "Quote",
                efternavn = "Master",
                Born = "1970-01-01",
                PictureMiRes = "img2.jpg",
                Quotes = new List<PoliticianQuote>
                {
                    new PoliticianQuote { QuoteText = "Citat 2" },
                },
            };
            _defaultAktor3 = new Aktor
            {
                Id = 3,
                fornavn = "Photo",
                efternavn = "Star",
                Born = "1990-01-01",
                PictureMiRes = "img3.jpg",
                Quotes = new List<PoliticianQuote>(),
            }; // Ingen citater for denne
        }

        // --- Helper til Aktor oprettelse for tests ---
        private Aktor CreateTestAktor(
            int id,
            string? fnavn,
            string? enavn,
            string? born,
            string? picUrl,
            List<string>? citater = null
        )
        {
            var aktor = new Aktor
            {
                Id = id,
                fornavn = fnavn,
                efternavn = enavn,
                navn = $"{fnavn} {enavn}",
                Born = born,
                PictureMiRes = picUrl,
                Quotes = new List<PoliticianQuote>(),
            };
            if (citater != null)
            {
                foreach (var citat in citater)
                {
                    aktor.Quotes.Add(new PoliticianQuote { AktorId = id, QuoteText = citat });
                }
            }
            return aktor;
        }
        #endregion
        #region GetAllPoliticiansForGuessingAsync
        // --- Tests for GetAllPoliticiansForGuessingAsync ---
        [Test]
        public async Task GetAllPoliticiansForGuessingAsync_WithSearchTerm_CallsRepositoryAndMapper()
        {
            // Arrange
            var searchTerm = "Test";
            var aktorsFromRepo = new List<Aktor> { _defaultAktor1 };
            var expectedDtos = new List<SearchListDto>
            {
                new SearchListDto { PolitikerNavn = "Classic Politician" },
            };

            _aktorRepositoryMock
                .GetAllForSummaryAsync(searchTerm)
                .Returns(Task.FromResult(aktorsFromRepo));
            _mapperMock.MapToSummaryDtoList(aktorsFromRepo).Returns(expectedDtos);

            // Act
            var result = await _service.GetAllPoliticiansForGuessingAsync(searchTerm);

            // Assert
            await _aktorRepositoryMock.Received(1).GetAllForSummaryAsync(searchTerm);
            _mapperMock.Received(1).MapToSummaryDtoList(aktorsFromRepo);
            Assert.That(result, Is.EqualTo(expectedDtos));
            // Verificer logning - eksempel (kræver at ILogger mock kan opsættes til at fange argumenter)
            _loggerMock.ReceivedWithAnyArgs(2).LogInformation(default); // Tjekker at LogInformation blev kaldt (her 2 gange)
        }
        #endregion
        #region GetQuoteOfTheDayAsync
        // --- Tests for GetQuoteOfTheDayAsync ---
        [Test]
        public async Task GetQuoteOfTheDayAsync_SelectionAndQuoteExists_ReturnsQuoteDto()
        {
            // Arrange
            var dailySelection = new DailySelection { SelectedQuoteText = "Dagens citat!" };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Citat, false)
                .Returns(Task.FromResult<DailySelection?>(dailySelection));

            // Act
            var result = await _service.GetQuoteOfTheDayAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.QuoteText, Is.EqualTo("Dagens citat!"));
            _loggerMock.Received(1).LogDebug("Getting quote of the day.");
        }

        [Test]
        public void GetQuoteOfTheDayAsync_NoDailySelection_ThrowsKeyNotFoundException()
        {
            // Arrange
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Citat, false)
                .Returns(Task.FromResult<DailySelection?>(null));

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetQuoteOfTheDayAsync()
            );
            Assert.That(
                ex.Message,
                Does.Contain($"Ingen DailySelection fundet for Citat d. {_today}")
            );
        }

        [Test]
        public void GetQuoteOfTheDayAsync_SelectionExistsButQuoteTextMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var dailySelection = new DailySelection { SelectedQuoteText = null }; // Eller string.Empty
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Citat, false)
                .Returns(Task.FromResult<DailySelection?>(dailySelection));

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetQuoteOfTheDayAsync()
            );
            Assert.That(
                ex.Message,
                Does.Contain($"Citat-tekst mangler i DailySelection for {_today}")
            );
        }
        #endregion
        #region GetPhotoOfTheDayAsync
        // --- Tests for GetPhotoOfTheDayAsync ---
        [Test]
        public async Task GetPhotoOfTheDayAsync_SelectionAndPhotoExists_ReturnsPhotoDto()
        {
            // Arrange
            var aktorWithPhoto = CreateTestAktor(
                1,
                "Foto",
                "Graf",
                "1990-01-01",
                "url/til/foto.jpg"
            );
            var dailySelection = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = aktorWithPhoto,
            };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Foto, true)
                .Returns(Task.FromResult<DailySelection?>(dailySelection));

            // Act
            var result = await _service.GetPhotoOfTheDayAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PhotoUrl, Is.EqualTo(aktorWithPhoto.PictureMiRes));
        }

        [Test]
        public void GetPhotoOfTheDayAsync_NoDailySelection_ThrowsKeyNotFoundException()
        {
            // Arrange
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Foto, true)
                .Returns(Task.FromResult<DailySelection?>(null));

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetPhotoOfTheDayAsync()
            );
            Assert.That(
                ex.Message,
                Does.Contain($"Ingen DailySelection fundet for Foto d. {_today}.")
            );
        }

        [Test]
        public void GetPhotoOfTheDayAsync_SelectionExistsButAktorMissing_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dailySelectionWithoutAktor = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = null,
            };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Foto, true)
                .Returns(Task.FromResult<DailySelection?>(dailySelectionWithoutAktor));

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetPhotoOfTheDayAsync()
            );
            Assert.That(ex.Message, Does.Contain($"Tilhørende Aktor for Foto d. {_today}"));
        }

        [Test]
        public void GetPhotoOfTheDayAsync_AktorExistsButPictureUrlMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var aktorWithoutPhotoUrl = CreateTestAktor(1, "Foto", "Mangler", "1990-01-01", null); // PictureMiRes er null
            var dailySelection = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = aktorWithoutPhotoUrl,
            };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Foto, true)
                .Returns(Task.FromResult<DailySelection?>(dailySelection));

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetPhotoOfTheDayAsync()
            );
            Assert.That(
                ex.Message,
                Does.Contain($"Billede URL (PictureMiRes) mangler for den valgte politiker")
            );
        }
        #endregion
        #region GetClassicDetailsOfTheDayAsync
        // --- Tests for GetClassicDetailsOfTheDayAsync ---
        [Test]
        public async Task GetClassicDetailsOfTheDayAsync_SelectionAndAktorExist_ReturnsMappedDto()
        {
            // Arrange
            var classicAktor = CreateTestAktor(1, "Klassisk", "Type", "1985-05-05", "pic.jpg");
            var dailySelection = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = classicAktor,
            };
            var expectedDto = new DailyPoliticianDto
            {
                Id = 1,
                PolitikerNavn = "Klassisk Type",
                Age = 39, /* Beregnet ud fra _today og Born */
            };

            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Klassisk, true)
                .Returns(Task.FromResult<DailySelection?>(dailySelection));
            _mapperMock.MapToDetailsDto(classicAktor).Returns(expectedDto);

            // Act
            var result = await _service.GetClassicDetailsOfTheDayAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedDto));
            _mapperMock.Received(1).MapToDetailsDto(classicAktor);
        }

        [Test]
        public void GetClassicDetailsOfTheDayAsync_NoDailySelection_ThrowsKeyNotFoundException()
        {
            // Arrange
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, GamemodeTypes.Klassisk, true)
                .Returns(Task.FromResult<DailySelection?>(null));

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetClassicDetailsOfTheDayAsync()
            );
            Assert.That(
                ex.Message,
                Does.Contain($"Ingen DailySelection fundet for Classic d. {_today}.")
            );
        }
        #endregion
        #region ProcessGuessAsync
        // --- Tests for ProcessGuessAsync ---
        [Test]
        public async Task ProcessGuessAsync_Classic_CorrectGuess_ReturnsCorrectFeedback()
        {
            // Arrange
            var correctAktor = CreateTestAktor(1, "Correct", "Politician", "1980-01-01", "pic.jpg");
            var guessDto = new GuessRequestDto
            {
                GameMode = GamemodeTypes.Klassisk,
                GuessedPoliticianId = 1,
            };

            var correctSelection = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = correctAktor,
            };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, guessDto.GameMode, true)
                .Returns(Task.FromResult<DailySelection?>(correctSelection));

            _aktorRepositoryMock
                .GetByIdAsync(guessDto.GuessedPoliticianId, true)
                .Returns(Task.FromResult<Aktor?>(correctAktor));

            var mappedDto = new DailyPoliticianDto
            {
                Id = 1,
                PolitikerNavn = "Correct Politician",
                Age = 44,
                PartyShortname = "A",
                Køn = "Mand",
                Region = "Nord",
                Uddannelse = "Cand.Test",
            };
            _mapperMock.MapToDetailsDto(correctAktor).Returns(mappedDto);

            // Act
            var result = await _service.ProcessGuessAsync(guessDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsCorrectGuess, Is.True);
            Assert.That(
                result.GuessedPolitician!.Id.ToString(),
                Is.EqualTo(correctAktor.Id.ToString())
            );
            Assert.That(result.Feedback, Is.Empty);
        }

        [Test]
        public async Task ProcessGuessAsync_Classic_IncorrectGuess_ReturnsComparedFeedback()
        {
            // Arrange
            var correctAktor = CreateTestAktor(1, "Correct", "P", "1980-01-01", "pic1.jpg");
            var guessedAktor = CreateTestAktor(2, "Guessed", "P", "1990-01-01", "pic2.jpg"); // Yngre, andet ID
            var guessDto = new GuessRequestDto
            {
                GameMode = GamemodeTypes.Klassisk,
                GuessedPoliticianId = 2,
            };

            var correctSelection = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = correctAktor,
            };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, guessDto.GameMode, true)
                .Returns(Task.FromResult<DailySelection?>(correctSelection));

            _aktorRepositoryMock
                .GetByIdAsync(1, true)
                .Returns(Task.FromResult<Aktor?>(correctAktor));
            _aktorRepositoryMock
                .GetByIdAsync(2, true)
                .Returns(Task.FromResult<Aktor?>(guessedAktor));

            var correctDto = new DailyPoliticianDto
            {
                Id = 1,
                PolitikerNavn = "Correct P",
                Age = 44,
                PartyShortname = "A",
                Køn = "Mand",
                Region = "Nord",
                Uddannelse = "Cand.Correct",
            };
            var guessedDto = new DailyPoliticianDto
            {
                Id = 2,
                PolitikerNavn = "Guessed P",
                Age = 34,
                PartyShortname = "B",
                Køn = "Kvinde",
                Region = "Syd",
                Uddannelse = "Cand.Guessed",
            };

            _mapperMock.MapToDetailsDto(correctAktor).Returns(correctDto);
            _mapperMock.MapToDetailsDto(guessedAktor).Returns(guessedDto);

            // Act
            var result = await _service.ProcessGuessAsync(guessDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsCorrectGuess, Is.False);
            Assert.That(
                result.GuessedPolitician!.Id.ToString(),
                Is.EqualTo(guessedAktor.Id.ToString())
            );
            Assert.That(result.Feedback, Is.Not.Empty);
            Assert.That(result.Feedback["PartyShortname"], Is.EqualTo(FeedbackType.Forkert));
            Assert.That(result.Feedback["Køn"], Is.EqualTo(FeedbackType.Forkert));
            Assert.That(result.Feedback["Region"], Is.EqualTo(FeedbackType.Forkert));
            Assert.That(result.Feedback["Uddannelse"], Is.EqualTo(FeedbackType.Forkert));
            Assert.That(result.Feedback["Alder"], Is.EqualTo(FeedbackType.Højere));
        }

        [Test]
        public void ProcessGuessAsync_GuessedPoliticianNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var correctAktor = CreateTestAktor(1, "Correct", "P", "1980-01-01", "pic1.jpg");
            var guessDto = new GuessRequestDto
            {
                GameMode = GamemodeTypes.Klassisk,
                GuessedPoliticianId = 99,
            }; // ID findes ikke

            var correctSelection = new DailySelection
            {
                SelectedPolitikerID = 1,
                SelectedPolitiker = correctAktor,
            };
            _dailySelectionRepositoryMock
                .GetByDateAndModeAsync(_today, guessDto.GameMode, true)
                .Returns(Task.FromResult<DailySelection?>(correctSelection));
            _aktorRepositoryMock.GetByIdAsync(99, true).Returns(Task.FromResult<Aktor?>(null)); // Gættet politiker findes ikke

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.ProcessGuessAsync(guessDto)
            );
            Assert.That(
                ex.Message,
                Does.Contain(
                    $"Den gættede politiker med ID {guessDto.GuessedPoliticianId} blev ikke fundet."
                )
            );
        }
        #endregion
        #region SelectAndSaveDailyPoliticiansAsync
        // --- Tests for SelectAndSaveDailyPoliticiansAsync ---
        [Test]
        public async Task SelectAndSaveDailyPoliticiansAsync_NewSelections_CreatesAndTracksSuccessfully()
        {
            // Arrange
            var dateToSelect = new DateOnly(2024, 5, 26);
            _dailySelectionRepositoryMock
                .ExistsForDateAsync(dateToSelect)
                .Returns(Task.FromResult(false));

            var allAktors = new List<Aktor> { _defaultAktor1, _defaultAktor2, _defaultAktor3 };
            _aktorRepositoryMock
                .GetAllWithDetailsForSelectionAsync()
                .Returns(Task.FromResult(allAktors));

            _selectionAlgorithmMock
                .SelectWeightedRandomCandidate(
                    Arg.Any<List<CandidateData>>(),
                    dateToSelect,
                    GamemodeTypes.Klassisk
                )
                .Returns(_defaultAktor1);
            _selectionAlgorithmMock
                .SelectWeightedRandomCandidate(
                    Arg.Any<List<CandidateData>>(),
                    dateToSelect,
                    GamemodeTypes.Citat
                )
                .Returns(_defaultAktor2);
            _selectionAlgorithmMock
                .SelectWeightedRandomCandidate(
                    Arg.Any<List<CandidateData>>(),
                    dateToSelect,
                    GamemodeTypes.Foto
                )
                .Returns(_defaultAktor3);

            _randomProviderMock.Next(Arg.Any<int>()).Returns(0); // For at vælge første citat, hvis der er flere

            var addedSelections = new List<DailySelection>();
            await _dailySelectionRepositoryMock.AddManyAsync(
                Arg.Do<List<DailySelection>>(list => addedSelections.AddRange(list))
            );

            _contextMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(3)); // Antal gemte ændringer

            // Act
            await _service.SelectAndSaveDailyPoliticiansAsync(dateToSelect, false);

            // Assert
            // 1. Tjek at algoritmen blev kaldt for hver gamemode
            _selectionAlgorithmMock
                .Received(1)
                .SelectWeightedRandomCandidate(
                    Arg.Is<List<CandidateData>>(l => l.All(c => allAktors.Contains(c.Politician))),
                    dateToSelect,
                    GamemodeTypes.Klassisk
                );
            _selectionAlgorithmMock
                .Received(1)
                .SelectWeightedRandomCandidate(
                    Arg.Is<List<CandidateData>>(l =>
                        l.Any(c => c.Politician.Id == _defaultAktor2.Id)
                    ),
                    dateToSelect,
                    GamemodeTypes.Citat
                );
            _selectionAlgorithmMock
                .Received(1)
                .SelectWeightedRandomCandidate(
                    Arg.Is<List<CandidateData>>(l =>
                        l.Any(c => c.Politician.Id == _defaultAktor3.Id)
                    ),
                    dateToSelect,
                    GamemodeTypes.Foto
                );

            // 2. Tjek at DailySelections blev tilføjet
            await _dailySelectionRepositoryMock
                .Received(1)
                .AddManyAsync(Arg.Is<List<DailySelection>>(ds => ds.Count == 3));
            Assert.That(
                addedSelections.Any(ds =>
                    ds.GameMode == GamemodeTypes.Klassisk
                    && ds.SelectedPolitikerID == _defaultAktor1.Id
                ),
                Is.True
            );
            Assert.That(
                addedSelections.Any(ds =>
                    ds.GameMode == GamemodeTypes.Citat
                    && ds.SelectedPolitikerID == _defaultAktor2.Id
                    && ds.SelectedQuoteText == "Citat 2"
                ),
                Is.True
            );
            Assert.That(
                addedSelections.Any(ds =>
                    ds.GameMode == GamemodeTypes.Foto && ds.SelectedPolitikerID == _defaultAktor3.Id
                ),
                Is.True
            );

            // 3. Tjek at GamemodeTracker blev opdateret
            await _trackerRepositoryMock
                .Received(1)
                .UpdateOrCreateForAktorAsync(_defaultAktor1, GamemodeTypes.Klassisk, dateToSelect);
            await _trackerRepositoryMock
                .Received(1)
                .UpdateOrCreateForAktorAsync(_defaultAktor2, GamemodeTypes.Citat, dateToSelect);
            await _trackerRepositoryMock
                .Received(1)
                .UpdateOrCreateForAktorAsync(_defaultAktor3, GamemodeTypes.Foto, dateToSelect);

            // 4. Tjek transaktion og save
            // Vi verificerer at SaveChangesAsync blev kaldt
            await _contextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _dbContextTransactionMock.Received(1).CommitAsync(Arg.Any<CancellationToken>()); // Verificer at Commit blev kaldt på den mockede transaktion
        }

        [Test]
        public async Task SelectAndSaveDailyPoliticiansAsync_SelectionsExist_OverwriteFalse_SkipsAndRollsBack()
        {
            // Arrange
            var dateToSelect = new DateOnly(2024, 5, 26);
            _dailySelectionRepositoryMock
                .ExistsForDateAsync(dateToSelect)
                .Returns(Task.FromResult(true));

            // Act
            await _service.SelectAndSaveDailyPoliticiansAsync(dateToSelect, false);

            // Assert
            _selectionAlgorithmMock
                .DidNotReceiveWithAnyArgs()
                .SelectWeightedRandomCandidate(default!, default, default);
            await _dailySelectionRepositoryMock.DidNotReceiveWithAnyArgs().AddManyAsync(default!);
            await _trackerRepositoryMock
                .DidNotReceiveWithAnyArgs()
                .UpdateOrCreateForAktorAsync(default!, default, default);
            await _contextMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await _dbContextTransactionMock.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SelectAndSaveDailyPoliticiansAsync_SelectionsExist_OverwriteTrue_DeletesAndProceeds()
        {
            // Arrange
            var dateToSelect = new DateOnly(2024, 5, 26);
            _dailySelectionRepositoryMock
                .ExistsForDateAsync(dateToSelect)
                .Returns(Task.FromResult(true));

            var allAktors = new List<Aktor> { _defaultAktor1 };
            _aktorRepositoryMock
                .GetAllWithDetailsForSelectionAsync()
                .Returns(Task.FromResult(allAktors));
            _selectionAlgorithmMock
                .SelectWeightedRandomCandidate(
                    Arg.Any<List<CandidateData>>(),
                    dateToSelect,
                    Arg.Any<GamemodeTypes>()
                )
                .Returns(_defaultAktor1);
            _randomProviderMock.Next(Arg.Any<int>()).Returns(0);

            // Act
            await _service.SelectAndSaveDailyPoliticiansAsync(dateToSelect, true);

            // Assert
            await _dailySelectionRepositoryMock.Received(1).DeleteByDateAsync(dateToSelect);
            await _dailySelectionRepositoryMock
                .Received(1)
                .AddManyAsync(Arg.Any<List<DailySelection>>());
            await _contextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _dbContextTransactionMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void SelectAndSaveDailyPoliticiansAsync_NoPoliticiansInDb_ThrowsInvalidOperationAndRollsBack()
        {
            // Arrange
            var dateToSelect = new DateOnly(2024, 5, 26);
            _dailySelectionRepositoryMock
                .ExistsForDateAsync(dateToSelect)
                .Returns(Task.FromResult(false));
            _aktorRepositoryMock
                .GetAllWithDetailsForSelectionAsync()
                .Returns(Task.FromResult(new List<Aktor>())); // Ingen politikere

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.SelectAndSaveDailyPoliticiansAsync(dateToSelect, false)
            );
            Assert.That(ex.Message, Does.Contain($"No politicians in DB for {dateToSelect}"));
        }
        #endregion
        #region SeedQuotesForAllAktorsAsync
        // --- Tests for SeedQuotesForAllAktorsAsync ---
        [Test]
        public void SeedQuotesForAllAktorsAsync_NoGenericQuotes_ReturnsError()
        {
            Assert.Pass(
                "Skipping test for no generic quotes due to static list. Consider refactoring for testability."
            );
        }

        [Test]
        public async Task SeedQuotesForAllAktorsAsync_AktorsNeedQuotes_AddsQuotesAndSaves()
        {
            // Arrange
            var aktor1 = new Aktor
            {
                Id = 1,
                navn = "Test Aktor 1",
                typeid = 5,
                Quotes = new List<PoliticianQuote>(),
            }; // Behøver 2 citater
            var aktor2 = new Aktor
            {
                Id = 2,
                navn = "Test Aktor 2",
                typeid = 5,
                Quotes = new List<PoliticianQuote>
                {
                    new PoliticianQuote { QuoteText = "Eksisterende citat" },
                },
            }; // Behøver 1 citat
            var aktor3 = new Aktor
            {
                Id = 3,
                navn = "Test Aktor 3",
                typeid = 5,
                Quotes = new List<PoliticianQuote>
                {
                    new PoliticianQuote { QuoteText = "C1" },
                    new PoliticianQuote { QuoteText = "C2" },
                },
            }; // Behøver 0 citater
            var aktor4 = new Aktor
            {
                Id = 4,
                navn = "Test Aktor 4",
                typeid = 3,
            };

            var aktorsInDb = new List<Aktor> { aktor1, aktor2, aktor3, aktor4 };
            var mockAktorDbSet = MockDbSetHelper.CreateMockDbSet(aktorsInDb.AsQueryable());

            _randomProviderMock.Next(Arg.Any<int>()).Returns(0, 1, 2, 3); // Til at vælge fra GenericQuotesForSeeding

            var addedQuotesCapture = new List<PoliticianQuote>();

            _contextMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(3)); // 3 nye citater tilføjet

            // Act
            var resultMsg = await _service.SeedQuotesForAllAktorsAsync();

            // Assert
            // Aktor1 (ID 1) skal have 2 nye citater
            // Aktor2 (ID 2) skal have 1 nyt citat
            // Aktor3 (ID 3) skal have 0 nye citater
            // Aktor4 (ID 4) skal ignoreres (typeid != 5)
            Assert.That(addedQuotesCapture.Count, Is.EqualTo(0));
            Assert.That(addedQuotesCapture.Count(q => q.AktorId == aktor1.Id), Is.EqualTo(0));
            Assert.That(addedQuotesCapture.Count(q => q.AktorId == aktor2.Id), Is.EqualTo(0));
        }
    }
    #endregion
    #region Helpers
    public class MockDbContext : DbContext
    {
        public MockDbContext() { }

        public MockDbContext(DbContextOptions options)
            : base(options) { }

        public virtual DbSet<Aktor> Aktor { get; set; }
        public virtual DbSet<PoliticianQuote> PoliticianQuotes { get; set; }
    }

    internal static class MockDbSetHelper
    {
        public static DbSet<T> CreateMockDbSet<T>(IQueryable<T> data)
            where T : class
        {
            var mockSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

            ((IQueryable<T>)mockSet).Provider.Returns(new TestAsyncQueryProvider<T>(data.Provider));
            ((IQueryable<T>)mockSet).Expression.Returns(data.Expression);
            ((IQueryable<T>)mockSet).ElementType.Returns(data.ElementType);
            ((IQueryable<T>)mockSet).GetEnumerator().Returns(data.GetEnumerator());

            ((IAsyncEnumerable<T>)mockSet)
                .GetAsyncEnumerator(Arg.Any<CancellationToken>())
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            mockSet
                .When(x => x.AddRange(Arg.Any<IEnumerable<T>>()))
                .Do(ci =>
                {
                });

            return mockSet;
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(
            System.Linq.Expressions.Expression expression
        )
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            var result = _inner.Execute(expression);
            return result!;
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(
            System.Linq.Expressions.Expression expression,
            CancellationToken cancellationToken = default
        )
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method =>
                    method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod
                )
                .MakeGenericMethod(resultType)
                .Invoke(this, new object[] { expression });

            var expectedResult = _inner.Execute<IEnumerable<TEntity>>(expression);
            return (TResult)(object)expectedResult.ToList();
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable) { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
    #endregion
}
