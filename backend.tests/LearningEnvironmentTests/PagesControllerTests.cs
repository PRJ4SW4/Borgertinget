using backend.DTO.LearningEnvironment;
using backend.Services.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace backend.tests.LearningEnvironmentTests;

[TestFixture]
public class PagesControllerTests
{
    private ILearningPageService _mockPageService;
    private PagesController _uut;

    [SetUp]
    public void Setup()
    {
        _mockPageService = Substitute.For<ILearningPageService>();
        _uut = new PagesController(_mockPageService);
    }

    [Test]
    public async Task GetPagesStructure_ReturnsOkResult_WithStructure()
    {
        // Arrange
        var expectedStructure = new List<PageSummaryDTO>
        {
            new PageSummaryDTO { Id = 1, Title = "Page 1" },
        };
        _mockPageService
            .GetPagesStructureAsync()
            .Returns(Task.FromResult<IEnumerable<PageSummaryDTO>>(expectedStructure));

        // Act
        var result = await _uut.GetPagesStructure();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null, "okResult should not be null");
        if (okResult != null)
        {
            Assert.That(okResult.Value, Is.EqualTo(expectedStructure));
        }
    }

    [Test]
    public async Task GetPage_PageExists_ReturnsOkResult_WithPageDetails()
    {
        // Arrange
        var pageId = 1;
        var expectedPageDetail = new PageDetailDTO { Id = pageId, Title = "Test Page" };
        _mockPageService
            .GetPageDetailAsync(pageId)
            .Returns(Task.FromResult<PageDetailDTO?>(expectedPageDetail));

        // Act
        var result = await _uut.GetPage(pageId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null, "okResult should not be null");
        if (okResult != null)
        {
            Assert.That(okResult.Value, Is.EqualTo(expectedPageDetail));
        }
    }

    [Test]
    public async Task GetPage_PageDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var pageId = 99;
        _mockPageService.GetPageDetailAsync(pageId).Returns(Task.FromResult<PageDetailDTO?>(null));

        // Act
        var result = await _uut.GetPage(pageId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetSectionPageOrder_ReturnsOkResult_WithOrderedIds()
    {
        // Arrange
        var pageId = 1;
        var expectedOrder = new List<int> { 1, 2, 3 };
        _mockPageService.GetSectionPageOrderAsync(pageId).Returns(Task.FromResult(expectedOrder));

        // Act
        var result = await _uut.GetSectionPageOrder(pageId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null, "okResult should not be null");
        if (okResult != null)
        {
            Assert.That(okResult.Value, Is.EqualTo(expectedOrder));
        }
    }
}
