using backend.Controllers;
using backend.DTO.Flashcards;
using backend.Services.Flashcards;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace backend.tests.FlashcardTests;

[TestFixture]
public class FlashcardsControllerTests
{
    private IFlashcardService _mockFlashcardService;
    private FlashcardsController _uut;

    [SetUp]
    public void Setup()
    {
        _mockFlashcardService = Substitute.For<IFlashcardService>();
        _uut = new FlashcardsController(_mockFlashcardService);
    }

    [Test]
    public async Task GetCollections_ReturnsOkResult_WithCollections()
    {
        // Arrange
        var expectedCollections = new List<FlashcardCollectionSummaryDTO>
        {
            new FlashcardCollectionSummaryDTO
            {
                CollectionId = 1,
                Title = "Test Collection 1",
                DisplayOrder = 1,
            },
        };
        _mockFlashcardService
            .GetCollectionsAsync()
            .Returns(
                Task.FromResult<IEnumerable<FlashcardCollectionSummaryDTO>>(expectedCollections)
            );

        // Act
        var result = await _uut.GetCollections();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedCollections));
    }

    [Test]
    public async Task GetCollectionDetails_ReturnsOkResult_WithDetails()
    {
        // Arrange
        var collectionId = 1;
        var expectedDetails = new FlashcardCollectionDetailDTO
        {
            CollectionId = collectionId,
            Title = "Test Collection",
        };
        _mockFlashcardService
            .GetCollectionDetailsAsync(collectionId)
            .Returns(Task.FromResult<FlashcardCollectionDetailDTO?>(expectedDetails));

        // Act
        var result = await _uut.GetCollectionDetails(collectionId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedDetails));
    }

    [Test]
    public async Task GetCollectionDetails_ReturnsNotFound_WhenCollectionDoesNotExist()
    {
        // Arrange
        var collectionId = 99; // An ID that doesn't exist
        _mockFlashcardService
            .GetCollectionDetailsAsync(collectionId)
            .Returns(Task.FromResult<FlashcardCollectionDetailDTO?>(null));

        // Act
        var result = await _uut.GetCollectionDetails(collectionId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
