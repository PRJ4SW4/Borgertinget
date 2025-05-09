using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;
using backend.Models.Flashcards;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.Services
{
    [TestFixture]
    public class AdministratorServiceTests
    {
        private DataContext _context;
        private AdministratorService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new DataContext(options);
            _service = new AdministratorService(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Database.EnsureDeleted(); // clean after each test
            _context.Dispose();
        }

        #region Flashcard POST

        [Test]
        public async Task CreateCollectionAsync_SavesAndReturnsId()
        {
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "Folketinget",
                Description = "Beskrivelse",
                Flashcards = new List<FlashcardDTO>
                {
                    new FlashcardDTO
                    {
                        FrontContentType = "Text",
                        FrontText = "Mette Frederiksen",
                        BackContentType = "Text",
                        BackText = "Socialdemokratiet",
                    },
                },
            };

            var id = await _service.CreateCollectionAsync(dto);

            Assert.That(id, Is.GreaterThan(0));
            var collection = await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync(c => c.CollectionId == id);
            Assert.That(collection?.Title, Is.EqualTo("Folketinget"));
            Assert.That(collection?.Flashcards.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateCollectionAsync_WithMultipleFlashcards_SavesAll()
        {
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "Alternativet",
                Description = "Grøn omstilling",
                Flashcards = new List<FlashcardDTO>
                {
                    new FlashcardDTO
                    {
                        FrontContentType = "Text",
                        FrontText = "Josephine Fock",
                        BackContentType = "Text",
                        BackText = "Tidligere leder",
                    },
                    new FlashcardDTO
                    {
                        FrontContentType = "Text",
                        FrontText = "Franziska Rosenkilde",
                        BackContentType = "Text",
                        BackText = "Nuværende leder",
                    },
                },
            };

            var id = await _service.CreateCollectionAsync(dto);
            var collection = await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync(c => c.CollectionId == id);

            Assert.That(collection?.Flashcards.Count, Is.EqualTo(2));
        }

        #endregion

        #region  Flashcard GET

        [Test]
        public async Task GetAllFlashcardCollectionTitlesAsync_CollectionsExist_ReturnsList()
        {
            _context.FlashcardCollections.AddRange(
                new FlashcardCollection { Title = "LA" },
                new FlashcardCollection { Title = "Nye Borgerlige" }
            );
            await _context.SaveChangesAsync();

            var titles = await _service.GetAllFlashcardCollectionTitlesAsync();

            Assert.That(titles.Count, Is.EqualTo(2));
            Assert.That(titles, Does.Contain("LA"));
        }

        [Test]
        public void GetAllFlashcardCollectionTitlesAsync_NoCollections_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.GetAllFlashcardCollectionTitlesAsync();
            });
        }

        [Test]
        public void GetFlashCardCollectionByTitle_InvalidTitle_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.GetFlashCardCollectionByTitle("Ukendt Titel");
            });
        }

        [Test]
        public async Task GetFlashCardCollectionByTitle_ValidTitle_ReturnsDto()
        {
            var collection = new FlashcardCollection
            {
                Title = "Dansk Folkeparti",
                Description = "Nationalt fokus",
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        FrontContentType = FlashcardContentType.Text,
                        FrontText = "Morten Messerschmidt",
                        BackContentType = FlashcardContentType.Text,
                        BackText = "Formand",
                    },
                },
            };

            _context.FlashcardCollections.Add(collection);
            await _context.SaveChangesAsync();

            var result = await _service.GetFlashCardCollectionByTitle("Dansk Folkeparti");

            Assert.That(result.Title, Is.EqualTo("Dansk Folkeparti"));
            Assert.That(result.Flashcards.Count, Is.EqualTo(1));
        }

        #endregion

        #region Flashcard PUT

        [Test]
        public async Task UpdateCollectionInfoAsync_ValidUpdate_ChangesTitleAndDescription()
        {
            var collection = new FlashcardCollection
            {
                Title = "SF",
                Description = "Tidligere titel",
            };
            _context.FlashcardCollections.Add(collection);
            await _context.SaveChangesAsync();

            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "SF Opdateret",
                Description = "Ny beskrivelse",
                Flashcards = new List<FlashcardDTO>
                {
                    new FlashcardDTO
                    {
                        FrontContentType = "Text",
                        FrontText = "Pia Olsen Dyhr",
                        BackContentType = "Text",
                        BackText = "Partiformand",
                    },
                },
            };

            await _service.UpdateCollectionInfoAsync(collection.CollectionId, dto);

            var updated = await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync(c => c.CollectionId == collection.CollectionId);

            Assert.That(updated?.Title, Is.EqualTo("SF Opdateret"));
            Assert.That(updated?.Flashcards.Count, Is.EqualTo(1));
        }

        [Test]
        public void UpdateCollectionInfoAsync_NonExistingId_ThrowsKeyNotFound()
        {
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "Fake",
                Description = "Should fail",
                Flashcards = new List<FlashcardDTO>(),
            };

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.UpdateCollectionInfoAsync(-1, dto);
            });
        }

        #endregion

        #region Flashcard DELETE

        [Test]
        public async Task DeleteFlashcardCollectionAsync_ValidId_RemovesCollection()
        {
            var collection = new FlashcardCollection
            {
                Title = "Moderaterne",
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        FrontContentType = FlashcardContentType.Text,
                        FrontText = "Lars Løkke",
                        BackContentType = FlashcardContentType.Text,
                        BackText = "Statsminister?",
                    },
                },
            };

            _context.FlashcardCollections.Add(collection);
            await _context.SaveChangesAsync();

            await _service.DeleteFlashcardCollectionAsync(collection.CollectionId);

            var deleted = await _context.FlashcardCollections.FindAsync(collection.CollectionId);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void DeleteFlashcardCollectionAsync_InvalidId_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.DeleteFlashcardCollectionAsync(-99);
            });
        }

        #endregion

        #region Username GET

        [Test]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser()
        {
            var user = new User
            {
                UserName = "hellethorning",
                Email = "helle@folketinget.dk",
                PasswordHash = "hashed_pw",
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserByUsernameAsync("hellethorning");

            Assert.That(result.UserName, Is.EqualTo("hellethorning"));
        }

        [Test]
        public void GetUserByUsernameAsync_NonExistingUser_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.GetUserByUsernameAsync("ukendtbruger");
            });
        }

        [Test]
        public async Task GetAllUsersAsync_WithUsers_ReturnsAll()
        {
            _context.Users.AddRange(
                new User
                {
                    Email = "karsten@folketinget.dk",
                    UserName = "margrethev",
                    PasswordHash = "dummyHash123",
                },
                new User
                {
                    Email = "karsten@folketinget.dk",
                    UserName = "larslykke",
                    PasswordHash = "dummyHash123",
                }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetAllUsersAsync();

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result.Any(u => u.UserName == "larslykke"));
        }

        [Test]
        public void GetAllUsersAsync_NoUsers_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.GetAllUsersAsync();
            });
        }

        #endregion

        #region Username PUT

        [Test]
        public async Task UpdateUserNameAsync_ValidId_UpdatesUserName()
        {
            var user = new User
            {
                Email = "karsten@folketinget.dk",
                UserName = "nybruger",
                PasswordHash = "dummyHash123",
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new UpdateUserNameDto { UserName = "nybruger" };

            await _service.UpdateUserNameAsync(user.Id, dto);

            var updated = await _context.Users.FindAsync(user.Id);
            Assert.That(updated?.UserName, Is.EqualTo("nybruger"));
        }

        [Test]
        public void UpdateUserNameAsync_InvalidId_ThrowsKeyNotFound()
        {
            var dto = new UpdateUserNameDto { UserName = "ingenbruger" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.UpdateUserNameAsync(-1, dto);
            });
        }

        #endregion

        #region Citat-mode GET

        [Test]
        public async Task GetAllQuotesAsync_WithQuotes_ReturnsList()
        {
            _context.PoliticianQuotes.Add(
                new PoliticianQuote { QuoteId = 1, QuoteText = "Vi har styr på økonomien" }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetAllQuotesAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].QuoteText, Is.EqualTo("Vi har styr på økonomien"));
        }

        [Test]
        public void GetAllQuotesAsync_NoQuotes_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.GetAllQuotesAsync();
            });
        }

        [Test]
        public async Task GetQuoteByIdAsync_ValidId_ReturnsQuote()
        {
            _context.PoliticianQuotes.Add(
                new PoliticianQuote { QuoteId = 7, QuoteText = "Mette sagde noget klogt" }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetQuoteByIdAsync(7);

            Assert.That(result.QuoteId, Is.EqualTo(7));
            Assert.That(result.QuoteText, Is.EqualTo("Mette sagde noget klogt"));
        }

        [Test]
        public void GetQuoteByIdAsync_InvalidId_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.GetQuoteByIdAsync(999);
            });
        }

        #endregion

        #region Citat-mode PUT

        [Test]
        public async Task EditQuoteAsync_ValidId_UpdatesQuoteText()
        {
            var quote = new PoliticianQuote { QuoteId = 3, QuoteText = "Originalt citat" };
            _context.PoliticianQuotes.Add(quote);
            await _context.SaveChangesAsync();

            await _service.EditQuoteAsync(3, "Nyt citat fra oppositionen");

            var updated = await _context.PoliticianQuotes.FindAsync(3);
            Assert.That(updated?.QuoteText, Is.EqualTo("Nyt citat fra oppositionen"));
        }

        [Test]
        public void EditQuoteAsync_InvalidId_ThrowsKeyNotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.EditQuoteAsync(999, "Dette virker ikke");
            });
        }

        #endregion
    }
}
