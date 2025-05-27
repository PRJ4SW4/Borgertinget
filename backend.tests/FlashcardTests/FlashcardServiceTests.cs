using backend.Models.Flashcards;
using backend.Repositories.Flashcards;
using backend.Services.Flashcards;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace backend.tests.FlashcardTests;

[TestFixture]
public class FlashcardServiceTests
{
    private IFlashcardRepository _mockFlashcardRepository;
    private ILogger<FlashcardService> _mockLogger;
    private FlashcardService _uut;

    [SetUp]
    public void Setup()
    {
        _mockFlashcardRepository = Substitute.For<IFlashcardRepository>();
        _mockLogger = Substitute.For<ILogger<FlashcardService>>();
        _uut = new FlashcardService(_mockFlashcardRepository, _mockLogger);
    }

    [Test]
    public async Task GetCollectionsAsync_ReturnsCollectionSummaries()
    {
        // Arrange
        var collectionsFromRepo = new List<FlashcardCollection>
        {
            new FlashcardCollection
            {
                CollectionId = 1,
                Title = "Collection 1",
                DisplayOrder = 1,
            },
            new FlashcardCollection
            {
                CollectionId = 2,
                Title = "Collection 2",
                DisplayOrder = 2,
            },
        };
        _mockFlashcardRepository
            .GetCollectionsAsync()
            .Returns(Task.FromResult<IEnumerable<FlashcardCollection>>(collectionsFromRepo));

        // Act
        var result = await _uut.GetCollectionsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        var firstCollection = result.First();
        Assert.That(
            firstCollection.CollectionId,
            Is.EqualTo(collectionsFromRepo.First().CollectionId)
        );
        Assert.That(firstCollection.Title, Is.EqualTo(collectionsFromRepo.First().Title));
        Assert.That(
            firstCollection.DisplayOrder,
            Is.EqualTo(collectionsFromRepo.First().DisplayOrder)
        );
    }

    [Test]
    public async Task GetCollectionDetailsAsync_ReturnsCollectionDetails_WhenCollectionExists()
    {
        // Arrange
        var collectionId = 1;
        var collectionFromRepo = new FlashcardCollection
        {
            CollectionId = collectionId,
            Title = "Test Collection",
            Description = "Test Description",
            Flashcards = new List<Flashcard>
            {
                new Flashcard
                {
                    FlashcardId = 1,
                    FrontText = "Q1",
                    BackText = "A1",
                    DisplayOrder = 1,
                    FrontContentType = FlashcardContentType.Text,
                    BackContentType = FlashcardContentType.Text,
                },
                new Flashcard
                {
                    FlashcardId = 2,
                    FrontText = "Q2",
                    BackText = "A2",
                    DisplayOrder = 2,
                    FrontContentType = FlashcardContentType.Text,
                    BackContentType = FlashcardContentType.Text,
                },
            },
        };
        _mockFlashcardRepository
            .GetCollectionDetailsAsync(collectionId)
            .Returns(Task.FromResult<FlashcardCollection?>(collectionFromRepo));

        // Act
        var result = await _uut.GetCollectionDetailsAsync(collectionId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CollectionId, Is.EqualTo(collectionFromRepo.CollectionId));
        Assert.That(result.Title, Is.EqualTo(collectionFromRepo.Title));
        Assert.That(result.Description, Is.EqualTo(collectionFromRepo.Description));
        Assert.That(result.Flashcards.Count, Is.EqualTo(collectionFromRepo.Flashcards.Count));
        Assert.That(
            result.Flashcards.First().FrontText,
            Is.EqualTo(collectionFromRepo.Flashcards.First().FrontText)
        );
    }

    [Test]
    public async Task GetCollectionDetailsAsync_ReturnsNull_WhenCollectionDoesNotExist()
    {
        // Arrange
        var collectionId = 99; // An ID that doesn't exist
        _mockFlashcardRepository
            .GetCollectionDetailsAsync(collectionId)
            .Returns(Task.FromResult<FlashcardCollection?>(null));

        // Act
        var result = await _uut.GetCollectionDetailsAsync(collectionId);

        // Assert
        Assert.That(result, Is.Null);
    }
}
