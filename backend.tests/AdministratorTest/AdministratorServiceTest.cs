using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.Flashcards;
using backend.DTOs;
using backend.Models;
using backend.Models.Flashcards;
using backend.Repositories;
using backend.Services;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Services
{
    [TestFixture]
    public class AdministratorServiceTests
    {
        private IAdministratorRepository _repository;
        private AdministratorService _service;

        [SetUp]
        public void Setup()
        {
            _repository = Substitute.For<IAdministratorRepository>();
            _service = new AdministratorService(_repository);
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
                        FrontText = "MF",
                        BackContentType = "Text",
                        BackText = "S",
                    },
                },
            };

            _repository
                .When(r => r.AddFlashcardCollectionAsync(Arg.Any<FlashcardCollection>()))
                .Do(ci => ci.Arg<FlashcardCollection>().CollectionId = 42);

            var id = await _service.CreateCollectionAsync(dto);
            Assert.That(id, Is.EqualTo(42));
        }

        [Test]
        public async Task CreateCollectionAsync_WithMultipleFlashcards_SendsAllFlashcardsToRepo()
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
                        FrontText = "A",
                        BackContentType = "Text",
                        BackText = "B",
                    },
                    new FlashcardDTO
                    {
                        FrontContentType = "Text",
                        FrontText = "C",
                        BackContentType = "Text",
                        BackText = "D",
                    },
                },
            };

            FlashcardCollection captured = null!;
            _repository
                .When(r => r.AddFlashcardCollectionAsync(Arg.Any<FlashcardCollection>()))
                .Do(ci => captured = ci.Arg<FlashcardCollection>());

            await _service.CreateCollectionAsync(dto);
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.Flashcards.Count, Is.EqualTo(2));
        }

        #endregion

        #region Flashcard GET

        [Test]
        public async Task GetAllFlashcardCollectionTitlesAsync_CollectionsExist_ReturnsList()
        {
            var titles = new List<string> { "LA", "NB" };
            _repository.GetAllFlashcardCollectionTitlesAsync().Returns(titles);

            var result = await _service.GetAllFlashcardCollectionTitlesAsync();
            Assert.That(result, Is.EquivalentTo(titles));
        }

        [Test]
        public void GetAllFlashcardCollectionTitlesAsync_NoCollections_Throws()
        {
            _repository.GetAllFlashcardCollectionTitlesAsync().Returns(new List<string>());
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetAllFlashcardCollectionTitlesAsync()
            );
        }

        [Test]
        public async Task GetFlashCardCollectionByTitle_Valid_ReturnsDto()
        {
            var entity = new FlashcardCollection
            {
                CollectionId = 1,
                Title = "DF",
                Description = "Nationalt",
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        FrontContentType = FlashcardContentType.Text,
                        FrontText = "MM",
                        BackContentType = FlashcardContentType.Text,
                        BackText = "Formand",
                    },
                },
            };
            _repository.GetFlashcardCollectionByTitleAsync("DF").Returns(entity);

            var dto = await _service.GetFlashCardCollectionByTitle("DF");
            Assert.That(dto.Title, Is.EqualTo("DF"));
            Assert.That(dto.Flashcards.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetFlashCardCollectionByTitle_Invalid_Throws()
        {
            _repository
                .GetFlashcardCollectionByTitleAsync("Ukendt")
                .Returns((FlashcardCollection)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetFlashCardCollectionByTitle("Ukendt")
            );
        }

        #endregion

        #region Flashcard PUT

        [Test]
        public async Task UpdateCollectionInfoAsync_Valid_UpdatesEntityAndRepoCalled()
        {
            var existing = new FlashcardCollection
            {
                CollectionId = 1,
                Title = "Old",
                Description = "Old",
                Flashcards = new List<Flashcard>(),
            };
            _repository.GetFlashcardCollectionByIdAsync(1).Returns(existing);

            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "New",
                Description = "New",
                Flashcards = new List<FlashcardDTO>
                {
                    new FlashcardDTO
                    {
                        FrontContentType = "Text",
                        FrontText = "X",
                        BackContentType = "Text",
                        BackText = "Y",
                    },
                },
            };

            await _service.UpdateCollectionInfoAsync(1, dto);
            await _repository.Received(1).UpdateFlashcardCollectionAsync(existing);
            Assert.That(existing.Title, Is.EqualTo("New"));
            Assert.That(existing.Flashcards.Count, Is.EqualTo(1));
        }

        [Test]
        public void UpdateCollectionInfoAsync_NonExisting_Throws()
        {
            _repository.GetFlashcardCollectionByIdAsync(7).Returns((FlashcardCollection)null!);
            var dto = new FlashcardCollectionDetailDTO
            {
                Title = "None",
                Description = "x",
                Flashcards = new List<FlashcardDTO>(),
            };
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateCollectionInfoAsync(7, dto)
            );
        }

        #endregion

        #region Flashcard DELETE

        [Test]
        public async Task DeleteFlashcardCollectionAsync_ValidId_CallsRepo()
        {
            var coll = new FlashcardCollection { CollectionId = 9 };
            _repository.GetFlashcardCollectionByIdAsync(9).Returns(coll);

            await _service.DeleteFlashcardCollectionAsync(9);
            await _repository.Received(1).DeleteFlashcardCollectionAsync(coll);
        }

        [Test]
        public void DeleteFlashcardCollectionAsync_InvalidId_Throws()
        {
            _repository.GetFlashcardCollectionByIdAsync(-9).Returns((FlashcardCollection)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteFlashcardCollectionAsync(-9)
            );
        }

        #endregion

        #region User

        [Test]
        public async Task GetUserByUsernameAsync_Existing_ReturnsUser()
        {
            var user = new User { Id = 1, UserName = "helle" };
            _repository.GetUserByUsernameAsync("helle").Returns(user);
            var res = await _service.GetUserIdByUsernameAsync("helle");
            Assert.That(res.UserId, Is.EqualTo(user.Id));
        }

        [Test]
        public void GetUserByUsernameAsync_NotFound_Throws()
        {
            _repository.GetUserByUsernameAsync("ghost").Returns((User)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetUserIdByUsernameAsync("ghost")
            );
        }

        [Test]
        public async Task UpdateUserNameAsync_Valid_UpdatesAndCallsRepo()
        {
            var user = new User { Id = 5, UserName = "old" };
            _repository.GetUserByIdAsync(5).Returns(user);
            await _service.UpdateUserNameAsync(5, new UpdateUserNameDto { UserName = "new" });
            Assert.That(user.UserName, Is.EqualTo("new"));
            await _repository.Received(1).UpdateUserAsync(user);
        }

        [Test]
        public void UpdateUserNameAsync_InvalidId_Throws()
        {
            _repository.GetUserByIdAsync(99).Returns((User)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateUserNameAsync(99, new UpdateUserNameDto { UserName = "x" })
            );
        }

        #endregion

        #region Quotes

        [Test]
        public async Task GetAllQuotesAsync_WithQuotes_ReturnsList()
        {
            var list = new List<PoliticianQuote>
            {
                new PoliticianQuote { QuoteId = 1, QuoteText = "Q" },
            };
            _repository.GetAllQuotesAsync().Returns(list);
            var res = await _service.GetAllQuotesAsync();
            Assert.That(res.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllQuotesAsync_None_Throws()
        {
            _repository.GetAllQuotesAsync().Returns(new List<PoliticianQuote>());
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAllQuotesAsync());
        }

        [Test]
        public async Task GetQuoteByIdAsync_Valid_ReturnsDto()
        {
            var quote = new PoliticianQuote { QuoteId = 7, QuoteText = "text" };
            _repository.GetQuoteByIdAsync(7).Returns(quote);
            var dto = await _service.GetQuoteByIdAsync(7);
            Assert.That(dto.QuoteText, Is.EqualTo("text"));
        }

        [Test]
        public void GetQuoteByIdAsync_InvalidId_Throws()
        {
            _repository.GetQuoteByIdAsync(999).Returns((PoliticianQuote)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetQuoteByIdAsync(999));
        }

        [Test]
        public async Task EditQuoteAsync_Valid_UpdatesAndCallsRepo()
        {
            var quote = new PoliticianQuote { QuoteId = 3, QuoteText = "Old" };
            _repository.GetQuoteByIdAsync(3).Returns(quote);

            await _service.EditQuoteAsync(3, "New");

            Assert.That(quote.QuoteText, Is.EqualTo("New"));
            await _repository.Received(1).UpdateQuoteAsync(quote);
        }

        [Test]
        public void EditQuoteAsync_InvalidId_Throws()
        {
            _repository.GetQuoteByIdAsync(404).Returns((PoliticianQuote)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.EditQuoteAsync(404, "text"));
        }

        #endregion

        #region Politician Twitter Lookup

        [Test]
        public async Task GetAktorIdByTwitterIdAsync_ValidId_ReturnsAktorId()
        {
            _repository.GetAktorIdByTwitterIdAsync(321).Returns(654);

            var result = await _service.GetAktorIdByTwitterIdAsync(321);

            Assert.That(result, Is.EqualTo(654));
        }

        [Test]
        public async Task GetAktorIdByTwitterIdAsync_IdNotFound_ReturnsNull()
        {
            _repository.GetAktorIdByTwitterIdAsync(999).Returns((int?)null);

            var result = await _service.GetAktorIdByTwitterIdAsync(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetAktorIdByTwitterIdAsync_InvalidId_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _service.GetAktorIdByTwitterIdAsync(0);
            });
        }

        #endregion
    }
}
