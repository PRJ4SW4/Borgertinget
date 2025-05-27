using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.FT;
using backend.Models.Politicians;
using backend.Repositories.Politicians;
using backend.Services.Politicians;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace backend.Services.Politicians.Tests
{
    [TestFixture]
    public class AktorServiceTests
    {
        private IAktorRepo _mockAktorRepo;
        private ILogger<AktorService> _mockLogger;
        private AktorService _uut;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockAktorRepo = Substitute.For<IAktorRepo>();
            _mockLogger = Substitute.For<ILogger<AktorService>>();

            // Instantiate the service with the mocks
            _uut = new AktorService(_mockAktorRepo, _mockLogger);
        }

        // --- Test for getById ---
        [Test]
        public async Task GetById_AktorExists_ReturnsAktorDetailDto()
        {
            // Arrange
            int testId = 1;
            var aktorEntity = new Aktor
            {
                Id = testId,
                navn = "Test Politician",
                fornavn = "Test",
                efternavn = "Politician",
                Party = "Test Party",
                PartyShortname = "TP",
                Sex = "Male",
                Born = "01-01-1980",
                opdateringsdato = DateTime.UtcNow,
            };
            _mockAktorRepo.GetAktorByIdAsync(testId).Returns(Task.FromResult<Aktor?>(aktorEntity));

            // Act
            var result = await _uut.getById(testId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(aktorEntity.Id));
            Assert.That(result.navn, Is.EqualTo(aktorEntity.navn));
            Assert.That(result.Party, Is.EqualTo(aktorEntity.Party));
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default(string));
        }

        [Test]
        public async Task GetById_AktorDoesNotExist_ReturnsNullAndLogsError()
        {
            // Arrange
            int testId = 99;
            _mockAktorRepo.GetAktorByIdAsync(testId).Returns(Task.FromResult<Aktor?>(null));

            // Act
            var result = await _uut.getById(testId);

            // Assert
            Assert.That(result, Is.Null);
        }

        // --- Test for getAllAktors ---
        [Test]
        public async Task GetAllAktors_WhenAktorsExist_ReturnsListOfAktorDetailDto()
        {
            // Arrange
            var aktorList = new List<Aktor>
            {
                new Aktor
                {
                    Id = 1,
                    navn = "Politician One",
                    opdateringsdato = DateTime.UtcNow,
                },
                new Aktor
                {
                    Id = 2,
                    navn = "Politician Two",
                    opdateringsdato = DateTime.UtcNow,
                },
            };
            _mockAktorRepo.AllAktorsToList().Returns(Task.FromResult(aktorList));

            // Act
            var result = await _uut.getAllAktors();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(aktorList.Count));
            Assert.That(result[0].navn, Is.EqualTo(aktorList[0].navn));
            Assert.That(result[1].navn, Is.EqualTo(aktorList[1].navn));
        }

        [Test]
        public async Task GetAllAktors_WhenNoAktorsExist_ReturnsEmptyList()
        {
            // Arrange
            var emptyAktorList = new List<Aktor>();
            _mockAktorRepo.AllAktorsToList().Returns(Task.FromResult(emptyAktorList));

            // Act
            var result = await _uut.getAllAktors();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        // --- Test for getByParty ---
        [Test]
        public async Task GetByParty_WhenAktorsExistForParty_ReturnsListOfAktorDetailDto()
        {
            // Arrange
            string testParty = "Test Party";
            var aktorList = new List<Aktor>
            {
                new Aktor
                {
                    Id = 1,
                    navn = "Politician Alpha",
                    Party = testParty,
                    opdateringsdato = DateTime.UtcNow,
                },
                new Aktor
                {
                    Id = 3,
                    navn = "Politician Gamma",
                    Party = testParty,
                    opdateringsdato = DateTime.UtcNow,
                },
            };
            _mockAktorRepo.GetAktorsByParty(testParty).Returns(Task.FromResult(aktorList));

            // Act
            var result = await _uut.getByParty(testParty);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(aktorList.Count));
            Assert.That(
                result.All(dto => dto.Party == testParty || dto.PartyShortname == testParty),
                Is.True
            );
        }

        [Test]
        public async Task GetByParty_WhenNoAktorsExistForParty_ReturnsEmptyList()
        {
            // Arrange
            string testParty = "NonExistent Party";
            var emptyAktorList = new List<Aktor>();
            _mockAktorRepo.GetAktorsByParty(testParty).Returns(Task.FromResult(emptyAktorList));

            // Act
            var result = await _uut.getByParty(testParty);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetById_MappingCorrectness_AllFieldsMapped()
        {
            // Arrange
            int testId = 5;
            var now = DateTime.UtcNow;
            var aktorEntity = new Aktor
            {
                Id = testId,
                fornavn = "Testy",
                efternavn = "McTestface",
                navn = "Testy McTestface",
                opdateringsdato = now,
                Party = "The Testers",
                PartyShortname = "TT",
                Sex = "Other",
                Born = "2000-01-01",
                EducationStatistic = "PhD in Testing",
                PictureMiRes = "http://example.com/pic.jpg",
                FunctionFormattedTitle = "Chief Test Officer",
                FunctionStartDate = "2023-01-01",
                PositionsOfTrust = "Head of Test Department",
                Email = "testy@example.com",
                MinisterTitel = "Minister of Tests",
                Ministers = new List<string>
                {
                    "Minister of Unit Tests",
                    "Minister of Integration Tests",
                },
                Spokesmen = new List<string> { "Spokesperson for Assertions" },
                ParliamentaryPositionsOfTrust = new List<string>
                {
                    "Chair of the Mocking Committee",
                },
                Constituencies = new List<string>
                {
                    "Test Constituency Alpha",
                    "Test Constituency Beta",
                },
                Nominations = new List<string> { "Nominated for Best Tester 2024" },
                Educations = new List<string> { "MSc Testology", "BSc Mocking" },
                Occupations = new List<string> { "Professional Test Writer" },
                PublicationTitles = new List<string> { "The Art of the Test", "NUnit for Dummies" },
            };
            _mockAktorRepo.GetAktorByIdAsync(testId).Returns(Task.FromResult<Aktor?>(aktorEntity));

            // Act
            var result = await _uut.getById(testId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(aktorEntity.Id));
            Assert.That(result.fornavn, Is.EqualTo(aktorEntity.fornavn));
            Assert.That(result.efternavn, Is.EqualTo(aktorEntity.efternavn));
            Assert.That(result.navn, Is.EqualTo(aktorEntity.navn));
            Assert.That(result.opdateringsdato, Is.EqualTo(aktorEntity.opdateringsdato));
            Assert.That(result.Party, Is.EqualTo(aktorEntity.Party));
            Assert.That(result.PartyShortname, Is.EqualTo(aktorEntity.PartyShortname));
            Assert.That(result.Sex, Is.EqualTo(aktorEntity.Sex));
            Assert.That(result.Born, Is.EqualTo(aktorEntity.Born));
            Assert.That(result.EducationStatistic, Is.EqualTo(aktorEntity.EducationStatistic));
            Assert.That(result.PictureMiRes, Is.EqualTo(aktorEntity.PictureMiRes));
            Assert.That(
                result.FunctionFormattedTitle,
                Is.EqualTo(aktorEntity.FunctionFormattedTitle)
            );
            Assert.That(result.FunctionStartDate, Is.EqualTo(aktorEntity.FunctionStartDate));
            Assert.That(result.PositionsOfTrust, Is.EqualTo(aktorEntity.PositionsOfTrust));
            Assert.That(result.Email, Is.EqualTo(aktorEntity.Email));
            Assert.That(result.Ministertitel, Is.EqualTo(aktorEntity.MinisterTitel));

            // Collection Assertions
            Assert.That(result.Ministers, Is.EqualTo(aktorEntity.Ministers));
            Assert.That(result.Spokesmen, Is.EqualTo(aktorEntity.Spokesmen));
            Assert.That(
                result.ParliamentaryPositionsOfTrust,
                Is.EqualTo(aktorEntity.ParliamentaryPositionsOfTrust)
            );
            Assert.That(result.Constituencies, Is.EqualTo(aktorEntity.Constituencies));
            Assert.That(result.Nominations, Is.EqualTo(aktorEntity.Nominations));
            Assert.That(result.Educations, Is.EqualTo(aktorEntity.Educations));
            Assert.That(result.Occupations, Is.EqualTo(aktorEntity.Occupations));
            Assert.That(result.PublicationTitles, Is.EqualTo(aktorEntity.PublicationTitles));
        }
    }
}
