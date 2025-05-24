using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.FT; // For ODA DTOs (CreateAktor, etc.) - though not directly used in this test setup
using backend.Models; // For PoliticianTwitterId
using backend.Models.Politicians; // For Aktor, Party
using backend.Repositories.Politicians;
using backend.Services.Politicians;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace backend.Services.Tests
{
    [TestFixture]
    public class FetchServiceIntegrationTests
    {
        private DataContext _context;
        private HttpService _httpService;
        private IConfiguration _configuration;
        private IAktorRepo _aktorRepo;
        private IPartyRepository _partyRepo;
        private ILogger<FetchService> _loggerFetchService;
        private ILogger<AktorRepo> _loggerAktorRepo;
        private ILogger<PartyRepository> _loggerPartyRepo;
        private FetchService _uut;

        [SetUp]
        public void SetUp()
        {
            // In-memory database
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;
            _context = new DataContext(options);

            // Logger mocks
            _loggerFetchService = Substitute.For<ILogger<FetchService>>();
            _loggerAktorRepo = Substitute.For<ILogger<AktorRepo>>();
            _loggerPartyRepo = Substitute.For<ILogger<PartyRepository>>();

            // Configuration mock for API URLs
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

            // Real HttpService
            _httpService = new HttpService();

            // Real Repositories with the in-memory context
            _aktorRepo = new AktorRepo(_context, _loggerAktorRepo);
            _partyRepo = new PartyRepository(_context, _loggerPartyRepo);

            // Service under test
            _uut = new FetchService(
                _context, // FetchService uses DataContext directly too
                _httpService,
                _configuration,
                _aktorRepo,
                _partyRepo,
                _loggerFetchService
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted(); // Clean up the in-memory database
            _context.Dispose();
        }

        [Test]
        [Category("Integration")]
        [Explicit("This test makes live API calls and can be slow/unreliable.")] // Mark as explicit due to live calls
        public async Task FetchAndUpdateAktorsAsync_WhenApisAreResponsive_ProcessesSomeData()
        {
            // Arrange
            // Optional: Pre-seed a PoliticianTwitterId to check if its AktorId gets linked.
            // This depends on a politician name that is highly likely to be returned by the live API.
            // Example (replace with a very common, active politician name if needed):
            string knownPoliticianName = "Mette Frederiksen"; // Adjust if needed for a stable test
            var twitterIdEntry = new PoliticianTwitterId
            {
                Name = knownPoliticianName,
                TwitterUserId = "testUser123",
                TwitterHandle = "testHandle",
            };
            _context.PoliticianTwitterIds.Add(twitterIdEntry);
            await _context.SaveChangesAsync();

            // Act
            var (totalAdded, totalUpdated, totalDeleted) = await _uut.FetchAndUpdateAktorsAsync();

            // Assert
            // Due to live API calls, we can't assert exact numbers easily.
            // We check that the process ran and some activity likely occurred.
            _loggerFetchService
                .Received()
                .LogInformation(Arg.Is<string>(s => s.Contains("Starting Aktor update process")));
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

            // Check if any Aktors were actually added or updated in the database
            var aktorsInDb = await _context.Aktor.ToListAsync();
            // This assertion is weak because if the DB was empty and API returned nothing new, it could be 0.
            // But if the API is working, it should find/create some.
            Assert.That(
                aktorsInDb,
                Is.Not.Empty,
                "Expected some Aktors to be processed into the database if the API is responsive."
            );

            // Check if any Parties were created
            var partiesInDb = await _context.Party.ToListAsync();
            // Similar to Aktors, this depends on API data.
            if (aktorsInDb.Any(a => !string.IsNullOrEmpty(a.Party)))
            {
                Assert.That(
                    partiesInDb,
                    Is.Not.Empty,
                    "Expected some Parties to be created if Aktors with party info were processed."
                );
            }

            // Check if the pre-seeded PoliticianTwitterId had its AktorId linked
            var updatedTwitterIdEntry = await _context.PoliticianTwitterIds.FirstOrDefaultAsync(p =>
                p.Name == knownPoliticianName
            );
            Assert.That(updatedTwitterIdEntry, Is.Not.Null);

            var linkedAktor = aktorsInDb.FirstOrDefault(a => a.navn == knownPoliticianName);
            if (linkedAktor != null) // Only assert if we found the Aktor in the DB
            {
                Assert.That(
                    updatedTwitterIdEntry.AktorId,
                    Is.EqualTo(linkedAktor.Id),
                    $"AktorId for '{knownPoliticianName}' in PoliticianTwitterIds should be linked."
                );
            }
        }

        [Test]
        public void FetchAndUpdateAktorsAsync_MissingApiConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var emptyConfiguration = new ConfigurationBuilder().Build(); // No API URLs

            var fetchServiceWithBadConfig = new FetchService(
                _context,
                _httpService,
                emptyConfiguration, // Use empty config
                _aktorRepo,
                _partyRepo,
                _loggerFetchService
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await fetchServiceWithBadConfig.FetchAndUpdateAktorsAsync()
            );
            Assert.That(
                ex.Message,
                Does.Contain("API URL configuration for Aktor update is incomplete")
            );
        }
    }
}
