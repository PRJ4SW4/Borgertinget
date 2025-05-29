using backend.DTO.FT;
using backend.Services.Politicians;

namespace backend.Services.Tests
{
    [TestFixture]
    public class HttpServiceIntegrationTests
    {
        private HttpService _uut;

        [SetUp]
        public void SetUp()
        {
            _uut = new HttpService();
        }

        // Using the ministerialTitles endpoint for tests
        [Test]
        [Category("Integration")]
        [Explicit("This makes calls to oda.ft, as to not misuse their endpoint")]
        public async Task GetJsonAsync_OdaApiMinisterTitles_ReturnsDeserializedData()
        {
            // Arrange
            string ministerTitlesUrl =
                "https://oda.ft.dk/api/Akt%C3%B8r?$filter=typeid%20eq%202&$select=id,gruppenavnkort";

            // Act
            var result = await _uut.GetJsonAsync<ODataResponse<MinisterialTitleDto>>(
                ministerTitlesUrl
            );

            // Assert
            Assert.That(
                result,
                Is.Not.Null,
                "The deserialized result (ODataResponse) should not be null."
            );
            Assert.That(
                result.Value,
                Is.Not.Null,
                "The 'Value' property (List<MinisterialTitleDto>) should not be null."
            );
            Assert.That(
                result.Value,
                Is.Not.Empty,
                "Expected the list of ministerial titles to not be empty."
            );

            var firstTitle = result.Value.FirstOrDefault();
            Assert.That(firstTitle, Is.Not.Null, "The first item in the list should not be null.");
            Assert.That(
                firstTitle.Id,
                Is.GreaterThan(0),
                "MinisterialTitleDto.Id should be greater than 0."
            );

            Assert.That(
                firstTitle.GruppenavnKort,
                Is.Not.Null.Or.Empty,
                "MinisterialTitleDto.GruppenavnKort should not be null or empty. Actual value: "
                    + (firstTitle.GruppenavnKort ?? "NULL")
            );
        }
    }
}
