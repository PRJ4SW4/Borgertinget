using backend.Models;
using backend.Models.Politicians;
using backend.Repositories.Politicians;
using backend.Services.Politicians;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace backend.Services.Tests
{
    [TestFixture]
    public class FetchServiceTests
    {
        private HttpService _httpService;
        private IConfiguration _configuration;
        private IAktorRepo _aktorRepoMock;
        private IPartyRepository _partyRepoMock;
        private ILogger<FetchService> _loggerFetchServiceMock;
        private FetchService _uut;

        [SetUp]
        public void SetUp()
        {
            // Logger mocks
            _loggerFetchServiceMock = Substitute.For<ILogger<FetchService>>();

            // Configuration for API URLs
            var inMemorySettings = new Dictionary<string, string?>
            {
                {
                    "Api:OdaApiPolitikere",
                    "https://oda.ft.dk/api/Akt%C3%B8r?$inlinecount=allpages&$filter=typeid%20eq%205"
                },
                {
                    "Api:OdaApiMinisterTitles",
                    "https://oda.ft.dk/api/Akt%C3%B8r?$filter=typeid%20eq%202&$select=id,gruppenavnkort"
                },
                {
                    "Api:OdaApiMinisterRelationships",
                    "https://oda.ft.dk/api/Akt%C3%B8rAkt%C3%B8r?$filter=rolleid%20eq%208%20and%20slutdato%20eq%20null&$select=fraaktørid,tilaktørid"
                },
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _httpService = new HttpService();

            // Mock Repositories
            _aktorRepoMock = Substitute.For<IAktorRepo>();
            _partyRepoMock = Substitute.For<IPartyRepository>();

            _uut = new FetchService(
                _httpService,
                _configuration,
                _aktorRepoMock,
                _partyRepoMock,
                _loggerFetchServiceMock
            );
        }

        [Test]
        [Category("Integration")]
        [Explicit(
            "This test makes live API calls and can be slow/unreliable. Focus is on FetchService interaction with mocked repos."
        )]
        public async Task FetchAndUpdateAktorsAsync_WhenApisAreResponsive_InteractsWithRepositoriesCorrectly()
        {
            // Arrange
            string knownPoliticianName = "Mette Frederiksen";
            var twitterIdEntry = new PoliticianTwitterId
            {
                Id = 138,
                Name = knownPoliticianName,
                TwitterUserId = "twitterUserMette",
                TwitterHandle = "metteHandle",
                AktorId = null,
            };

            _aktorRepoMock.GetAktorByIdAsync(Arg.Any<int>()).Returns(Task.FromResult<Aktor?>(null));

            _partyRepoMock.GetByName(Arg.Any<string>()).Returns(Task.FromResult<Party?>(null));

            _aktorRepoMock
                .GetPoliticianTwitterIdByNameAsync(knownPoliticianName)
                .Returns(Task.FromResult<PoliticianTwitterId?>(twitterIdEntry));

            _aktorRepoMock.SaveChangesAsync().Returns(Task.FromResult(1));
            _partyRepoMock.SaveChangesAsync().Returns(Task.FromResult(1));

            // Act
            var (totalAdded, totalUpdated, totalDeleted) = await _uut.FetchAndUpdateAktorsAsync();

            // Assert
            _loggerFetchServiceMock
                .Received()
                .LogInformation(Arg.Is<string>(s => s.Contains("Starting Aktor update process")));

            // Check return values (these depend on live API data, so broad checks)
            Assert.That(
                totalAdded,
                Is.GreaterThanOrEqualTo(0),
                "Total added should be non-negative."
            );
            Assert.That(
                totalUpdated,
                Is.GreaterThanOrEqualTo(0),
                "Total updated should be non-negative."
            );
            Assert.That(
                totalDeleted,
                Is.GreaterThanOrEqualTo(0),
                "Total deleted should be non-negative."
            );

            await _aktorRepoMock.ReceivedWithAnyArgs().AddAktor(Arg.Any<Aktor>());
            await _partyRepoMock.ReceivedWithAnyArgs().AddParty(Arg.Any<Party>());

            await _aktorRepoMock.Received().SaveChangesAsync();

            await _partyRepoMock.Received().SaveChangesAsync();

            await _aktorRepoMock.Received().GetPoliticianTwitterIdByNameAsync(knownPoliticianName);

            if (twitterIdEntry.AktorId.HasValue)
            {
                _loggerFetchServiceMock
                    .Received()
                    .LogInformation(
                        Arg.Is<string>(s =>
                            s.Contains(
                                $"Updated PoliticianTwitterId.AktorId for '{knownPoliticianName}'"
                            )
                        )
                    );
            }
        }

        [Test]
        public void FetchAndUpdateAktorsAsync_MissingApiConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var emptyConfiguration = new ConfigurationBuilder().Build();

            var fetchServiceWithBadConfig = new FetchService(
                _httpService,
                emptyConfiguration,
                _aktorRepoMock,
                _partyRepoMock,
                _loggerFetchServiceMock
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await fetchServiceWithBadConfig.FetchAndUpdateAktorsAsync()
            );
            Assert.That(
                ex?.Message,
                Does.Contain("API URL configuration for Aktor update is incomplete")
            );
        }
    }
}
