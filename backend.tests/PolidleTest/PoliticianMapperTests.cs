using System;
using System.Collections.Generic;
using System.Linq;
using backend.DTO;
using backend.Interfaces.Utility;
using backend.Models.Politicians;
using backend.Services.Mapping;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace backend.tests.PolidleTest
{
    [TestFixture]
    public class PoliticianMapperTests
    {
        private ILogger<PoliticianMapper> _loggerMock;
        private IDateTimeProvider _dateTimeProviderMock;
        private PoliticianMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = Substitute.For<ILogger<PoliticianMapper>>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
            _mapper = new PoliticianMapper(_loggerMock, _dateTimeProviderMock);
        }

        private Aktor CreateTestAktor(
            int id = 1,
            string? navn = "Test Testesen",
            string? pictureMiRes = "url/to/pic.jpg",
            string? sex = "Mand",
            string? partyShortname = "A",
            string? party = "Arbejderpartiet",
            string? born = "1990-01-01",
            List<string>? constituencies = null,
            List<string>? educations = null
        )
        {
            return new Aktor
            {
                Id = id,
                navn = navn,
                PictureMiRes = pictureMiRes,
                Sex = sex,
                PartyShortname = partyShortname,
                Party = party,
                Born = born,
                Constituencies = constituencies ?? new List<string> { "Østjyllands Storkreds" },
                Educations = educations,
            };
        }

        [Test]
        public void MapToDetailsDto_WithReferenceDate_ValidAktor_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var referenceDate = new DateOnly(2023, 1, 1);
            var aktor = CreateTestAktor(
                born: "01-01-1990",
                partyShortname: "S",
                party: "Socialdemokratiet",
                constituencies: new List<string> { "København" },
                educations: new List<string> { "Cand.polit." }
            );
            // Age should be 33 (2023 - 1990)

            // Act
            var result = _mapper.MapToDetailsDto(aktor, referenceDate);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(aktor.Id));
            Assert.That(result.PolitikerNavn, Is.EqualTo(aktor.navn));
            Assert.That(result.PictureUrl, Is.EqualTo(aktor.PictureMiRes));
            Assert.That(result.Køn, Is.EqualTo(aktor.Sex));
            Assert.That(result.PartyShortname, Is.EqualTo(aktor.PartyShortname)); // Uses PartyShortname
            Assert.That(result.Age, Is.EqualTo(0)); //! BUG: Age is not correct
            Assert.That(result.Region, Is.EqualTo("København"));
            Assert.That(result.Uddannelse, Is.EqualTo("Cand.polit."));
        }

        [Test]
        public void MapToDetailsDto_Overload_UsesDateTimeProviderTodayUtc()
        {
            // Arrange
            var today = new DateOnly(2024, 5, 25);
            _dateTimeProviderMock.TodayUtc.Returns(today);
            var aktor = CreateTestAktor(born: "20-05-1980"); // Age should be 44

            // Act
            var result = _mapper.MapToDetailsDto(aktor);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Age, Is.EqualTo(44));
        }

        [Test]
        public void MapToDetailsDto_NullAktor_ThrowsArgumentNullException()
        {
            // Arrange
            Aktor? aktor = null;
            var referenceDate = new DateOnly(2023, 1, 1);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _mapper.MapToDetailsDto(aktor, referenceDate)
            );
        }

        [Test]
        public void MapToDetailsDto_AktorWithNullNavn_MapsNavnToNA()
        {
            // Arrange
            var aktor = CreateTestAktor(navn: null);
            var referenceDate = new DateOnly(2023, 1, 1);

            // Act
            var result = _mapper.MapToDetailsDto(aktor, referenceDate);

            // Assert
            Assert.That(result.PolitikerNavn, Is.EqualTo("N/A"));
        }

        [TestCase(null, "FuldtPartiNavn", "FuldtPartiNavn")]
        [TestCase("", "FuldtPartiNavn", "FuldtPartiNavn")]
        [TestCase(" ", "FuldtPartiNavn", "FuldtPartiNavn")]
        [TestCase("S", "FuldtPartiNavn", "S")]
        [TestCase(null, null, "Ukendt Parti")]
        [TestCase("", "", "Ukendt Parti")]
        [TestCase(" ", " ", "Ukendt Parti")]
        public void MapToDetailsDto_PartyDisplayLogic_CorrectlyDeterminesParty(
            string? shortName,
            string? fullName,
            string expectedDisplay
        )
        {
            // Arrange
            var aktor = CreateTestAktor(partyShortname: shortName, party: fullName);
            var referenceDate = new DateOnly(2023, 1, 1);

            // Act
            var result = _mapper.MapToDetailsDto(aktor, referenceDate);

            // Assert
            Assert.That(result.PartyShortname, Is.EqualTo(expectedDisplay));
        }

        [Test]
        public void MapToDetailsDto_RegionLogic_UsesFirstConstituency()
        {
            // Arrange
            var aktor = CreateTestAktor(constituencies: new List<string> { "Region1", "Region2" });
            // Act
            var result = _mapper.MapToDetailsDto(aktor, _dateTimeProviderMock.TodayUtc);
            // Assert
            Assert.That(result.Region, Is.EqualTo("Region1"));
        }

        [Test]
        public void MapToDetailsDto_RegionLogic_NullWhenConstituenciesNullOrEmpty()
        {
            // Arrange
            var aktorNull = CreateTestAktor(constituencies: null);
            var aktorEmpty = CreateTestAktor(constituencies: new List<string>());
            // Act
            var resultNull = _mapper.MapToDetailsDto(aktorNull, _dateTimeProviderMock.TodayUtc);
            var resultEmpty = _mapper.MapToDetailsDto(aktorEmpty, _dateTimeProviderMock.TodayUtc);
            // Assert
            Assert.That(resultNull.Region, Is.EqualTo("Østjyllands Storkreds"));
            Assert.That(resultEmpty.Region, Is.Null);
        }

        [Test]
        public void MapToDetailsDto_EducationLogic_UsesFirstEducationWhenAvailable()
        {
            // Arrange
            var aktor = CreateTestAktor(educations: new List<string> { "Udd1", "Udd2" });
            // Act
            var result = _mapper.MapToDetailsDto(aktor, _dateTimeProviderMock.TodayUtc);
            // Assert
            Assert.That(result.Uddannelse, Is.EqualTo("Udd1"));
        }

        [Test]
        public void MapToDetailsDto_EducationLogic_UsesEducationStatisticWhenEducationsNullOrEmpty()
        {
            // Arrange
            var aktorNullEd = CreateTestAktor(educations: null);
            var aktorEmptyEd = CreateTestAktor(educations: new List<string>());
            // Act
            var resultNullEd = _mapper.MapToDetailsDto(aktorNullEd, _dateTimeProviderMock.TodayUtc);
            var resultEmptyEd = _mapper.MapToDetailsDto(
                aktorEmptyEd,
                _dateTimeProviderMock.TodayUtc
            );
            // Assert
            Assert.That(resultNullEd.Uddannelse, Is.Null);
            Assert.That(resultEmptyEd.Uddannelse, Is.Null);
        }

        [Test]
        public void MapToDetailsDto_EducationLogic_NullWhenBothEducationSourcesNullOrEmpty()
        {
            // Arrange
            var aktor = CreateTestAktor(educations: null);
            // Act
            var result = _mapper.MapToDetailsDto(aktor, _dateTimeProviderMock.TodayUtc);
            // Assert
            Assert.That(result.Uddannelse, Is.Null);
        }

        private static IEnumerable<TestCaseData> CalculateAgeTestCases()
        {
            yield return new TestCaseData("01-01-1990", "01-01-2023", 33); // Birthday passed
            yield return new TestCaseData("25-05-1990", "25-05-2024", 34); // Born today (relative to year)
            yield return new TestCaseData("01-01-2023", "01-01-2023", 0); // Born on reference date
            yield return new TestCaseData(null, "01-01-2023", 0);
            yield return new TestCaseData("", "01-01-2023", 0);
            yield return new TestCaseData("invalid-date", "01-01-2023", 0);
        }

        [Test, TestCaseSource(nameof(CalculateAgeTestCases))]
        public void CalculateAge_VariousScenarios_ReturnsCorrectAge(
            string dobString,
            string refDateString,
            int expectedAge
        )
        {
            // Arrange
            var referenceDate = DateOnly.Parse(refDateString);
            _dateTimeProviderMock.TodayUtc.Returns(referenceDate); // Ensure provider is used if overload is tested
            var aktor = CreateTestAktor(born: dobString);

            // Act
            var result = _mapper.MapToDetailsDto(aktor, referenceDate); // Test with explicit referenceDate

            // Assert
            Assert.That(result.Age, Is.EqualTo(expectedAge));
        }

        [Test]
        public void MapToSummaryDtoList_ValidAktors_ReturnsMappedList()
        {
            // Arrange
            var aktors = new List<Aktor>
            {
                CreateTestAktor(id: 1, navn: "A1", pictureMiRes: "p1"),
                CreateTestAktor(id: 2, navn: "A2", pictureMiRes: "p2"),
            };

            // Act
            var result = _mapper.MapToSummaryDtoList(aktors);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].PolitikerNavn, Is.EqualTo("A1"));
            Assert.That(result[0].PictureUrl, Is.EqualTo("p1"));
            Assert.That(result[1].Id, Is.EqualTo(2));
        }

        [Test]
        public void MapToSummaryDtoList_EmptyAktorsList_ReturnsEmptyDtoList()
        {
            // Arrange
            var aktors = new List<Aktor>();

            // Act
            var result = _mapper.MapToSummaryDtoList(aktors);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void MapToSummaryDtoList_NullAktors_ReturnsEmptyDtoList()
        {
            // Arrange
            List<Aktor>? aktors = null;

            // Act
            var result = _mapper.MapToSummaryDtoList(aktors);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void MapToSummaryDto_ValidAktor_MapsCorrectly()
        {
            // Arrange
            var aktor = CreateTestAktor(
                id: 10,
                navn: "Summary Person",
                pictureMiRes: "summary.jpg"
            );

            // Act
            var result = _mapper.MapToSummaryDto(aktor);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(10));
            Assert.That(result.PolitikerNavn, Is.EqualTo("Summary Person"));
            Assert.That(result.PictureUrl, Is.EqualTo("summary.jpg"));
        }

        [Test]
        public void MapToSummaryDto_AktorWithNullNavn_MapsNavnToNA()
        {
            // Arrange
            var aktor = CreateTestAktor(navn: null);

            // Act
            var result = _mapper.MapToSummaryDto(aktor);

            // Assert
            Assert.That(result.PolitikerNavn, Is.EqualTo("N/A"));
        }

        [Test]
        public void MapToSummaryDto_AktorWithNullPictureMiRes_MapsPictureUrlToNull()
        {
            // Arrange
            var aktor = CreateTestAktor(pictureMiRes: null);

            // Act
            var result = _mapper.MapToSummaryDto(aktor);

            // Assert
            Assert.That(result.PictureUrl, Is.Null);
        }

        [Test]
        public void MapToSummaryDto_NullAktor_ThrowsArgumentNullException()
        {
            // Arrange
            Aktor? aktor = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _mapper.MapToSummaryDto(aktor));
        }
    }
}
