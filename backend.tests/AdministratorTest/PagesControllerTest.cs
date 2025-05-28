using backend.Controllers;
using backend.DTO.LearningEnvironment;
using backend.Services.LearningEnvironment;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Tests.Controllers
{
    [TestFixture]
    public class PagesControllerAdminTests
    {
        private PagesController _uut;
        private ILearningPageService _pageService;

        [SetUp]
        public void SetUp()
        {
            _pageService = Substitute.For<ILearningPageService>();
            _uut = new PagesController(_pageService);
        }

        #region Learningenvironment POST
        [Test]
        public async Task CreatePage_ShouldReturnCreatedPage_WhenRequestIsValid()
        {
            // Arrange
            var createRequest = new PageCreateRequestDTO
            {
                // Populate with test data
            };
            var createdPage = new PageDetailDTO
            {
                Id = 1,
                // Populate with test data
            };
            _pageService.CreatePageAsync(createRequest).Returns(createdPage);

            // Act
            var result = await _uut.CreatePage(createRequest);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult?.Value, Is.EqualTo(createdPage));
        }

        #endregion

        #region Learningenvironment PUT

        [Test]
        public async Task UpdatePage_ShouldReturnNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateRequest = new PageUpdateRequestDTO
            {
                Id = 1,
                // Populate with test data
            };
            _pageService.UpdatePageAsync(updateRequest.Id, updateRequest).Returns(true);

            // Act
            var result = await _uut.UpdatePage(updateRequest.Id, updateRequest);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task UpdatePage_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var updateRequest = new PageUpdateRequestDTO
            {
                Id = 1,
                // Populate with test data
            };

            // Act
            var result = await _uut.UpdatePage(2, updateRequest);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdatePage_ShouldReturnNotFound_WhenPageDoesNotExist()
        {
            // Arrange
            var updateRequest = new PageUpdateRequestDTO
            {
                Id = 1,
                // Populate with test data
            };
            _pageService.UpdatePageAsync(updateRequest.Id, updateRequest).Returns(false);

            // Act
            var result = await _uut.UpdatePage(updateRequest.Id, updateRequest);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        #endregion

        #region Learningenvironment Delete

        [Test]
        public async Task DeletePage_ShouldReturnNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange
            var pageId = 1;
            _pageService.DeletePageAsync(pageId).Returns(true);

            // Act
            var result = await _uut.DeletePage(pageId);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeletePage_ShouldReturnNotFound_WhenPageDoesNotExist()
        {
            // Arrange
            var pageId = 1;
            _pageService.DeletePageAsync(pageId).Returns(false);

            // Act
            var result = await _uut.DeletePage(pageId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }
        #endregion
    }
}
