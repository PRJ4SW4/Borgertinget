using System.Security.Claims;
using backend.Controllers;
using backend.DTOs;
using backend.Services.Subscription;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Controllers
{
    [TestFixture]
    public class SubscriptionControllerTests
    {
        private SubscriptionController _controller;
        private ISubscriptionService _service;
        private ILogger<SubscriptionController> _logger;

        [SetUp]
        public void Setup()
        {
            _service = Substitute.For<ISubscriptionService>();
            _logger = Substitute.For<ILogger<SubscriptionController>>();

            _controller = new SubscriptionController(_service, _logger);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "42") };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user },
            };
        }

        [Test]
        public async Task Subscribe_ValidRequest_ReturnsOk()
        {
            var dto = new SubscribeDto { PoliticianId = 123 };
            _service
                .SubscribeAsync(42, 123)
                .Returns((true, "Successfully subscribed to politician"));

            var result = await _controller.Subscribe(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Successfully subscribed to politician"));
            await _service.Received(1).SubscribeAsync(42, 123);
        }

        [Test]
        public async Task Subscribe_PoliticianNotFound_ReturnsBadRequest()
        {
            var dto = new SubscribeDto { PoliticianId = 123 };
            _service.SubscribeAsync(42, 123).Returns((false, "Politician with ID 123 findes ikke"));

            var result = await _controller.Subscribe(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var bad = result as BadRequestObjectResult;
            Assert.That(bad?.Value, Is.EqualTo("Politician with ID 123 findes ikke"));
        }

        [Test]
        public async Task Subscribe_AlreadySubscribed_ReturnsConflict()
        {
            var dto = new SubscribeDto { PoliticianId = 123 };
            _service
                .SubscribeAsync(42, 123)
                .Returns((false, "Du abonnerer allerede på denne politiker"));

            var result = await _controller.Subscribe(dto);

            Assert.That(result, Is.TypeOf<ConflictObjectResult>());
            var conflict = result as ConflictObjectResult;
            Assert.That(conflict?.Value, Is.EqualTo("Du abonnerer allerede på denne politiker"));
        }

        [Test]
        public async Task Subscribe_Exception_Returns500()
        {
            var dto = new SubscribeDto { PoliticianId = 123 };
            _service.SubscribeAsync(42, 123).Throws(new Exception("Database connection failed"));

            var result = await _controller.Subscribe(dto);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public async Task Unsubscribe_ValidRequest_ReturnsOk()
        {
            int politicianId = 123;
            _service
                .UnsubscribeAsync(42, politicianId)
                .Returns((true, "Successfully unsubscribed from politician"));

            var result = await _controller.Unsubscribe(politicianId);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo("Successfully unsubscribed from politician"));
        }

        [Test]
        public async Task Unsubscribe_NotFound_ReturnsNotFound()
        {
            int politicianId = 123;
            _service.UnsubscribeAsync(42, politicianId).Returns((false, "Subscription not found"));

            var result = await _controller.Unsubscribe(politicianId);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = result as NotFoundObjectResult;
            Assert.That(notFound?.Value, Is.EqualTo("Subscription not found"));
        }

        [Test]
        public async Task GetPoliticianTwitterIdByAktorId_ValidRequest_ReturnsOk()
        {
            int aktorId = 789;
            var expectedPoliticianInfo = new PoliticianInfoDto { Id = 123, Name = "Testesen" };
            _service.LookupPoliticianAsync(aktorId).Returns(expectedPoliticianInfo);

            var result = await _controller.GetPoliticianTwitterIdByAktorId(aktorId);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = result.Result as OkObjectResult;
            Assert.That(ok?.Value, Is.EqualTo(expectedPoliticianInfo));
        }

        [Test]
        public async Task GetPoliticianTwitterIdByAktorId_NotFound_ReturnsNotFound()
        {
            int aktorId = 789;
            _service.LookupPoliticianAsync(aktorId).Returns(default(PoliticianInfoDto));

            var result = await _controller.GetPoliticianTwitterIdByAktorId(aktorId);

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = result.Result as NotFoundObjectResult;
            Assert.That(notFound?.Value, Is.EqualTo("Politiker ikke fundet."));
        }

        [Test]
        public async Task GetPoliticianTwitterIdByAktorId_InvalidId_ReturnsBadRequest()
        {
            int invalidAktorId = 0;

            var result = await _controller.GetPoliticianTwitterIdByAktorId(invalidAktorId);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest?.Value, Is.EqualTo("Ugyldigt Aktor ID."));
        }
    }
}
