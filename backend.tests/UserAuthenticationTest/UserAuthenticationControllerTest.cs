using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTO.UserAuthentication;
using backend.DTOs;
using backend.Models;
using backend.utils;
using backend.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Controllers
{
    [TestFixture]
    public class UsersControllerTests
    {
        private IUserAuthenticationService _mockUserAuthService;
        private IEmailService _mockEmailService;
        private IConfiguration _mockConfiguration;
        private ILogger<UsersController> _mockLogger;
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserAuthService = Substitute.For<IUserAuthenticationService>();
            _mockConfiguration = Substitute.For<IConfiguration>();
            var mockEmailServiceLogger = Substitute.For<ILogger<EmailService>>();
            _mockEmailService = Substitute.For<IEmailService>(); 


            _mockLogger = Substitute.For<ILogger<UsersController>>();

            _controller = new UsersController(
                _mockConfiguration,
                _mockEmailService,
                _mockLogger,
                _mockUserAuthService
            );

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Name, "testuser"),
                    },
                    "mock"
                )
            );
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user },
            };

            // Mock IUrlHelper
            var mockUrlHelper = Substitute.For<IUrlHelper>();
            mockUrlHelper
                .Action(Arg.Any<UrlActionContext>()) // Matcher ethvert kald til Action
                .Returns("http://localhost/fakeaction"); // Returner en dummy URL

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user },
            };
            _controller.Url = mockUrlHelper; // Sæt den mockede UrlHelper på controlleren
        }

        [Test]
        public async Task CreateUser_ValidDto_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
            };
            var identityResultSucceeded = IdentityResult.Success;
            var user = new User
            {
                Id = 1,
                UserName = registerDto.Username,
                Email = registerDto.Email,
            };
            var token = "dummyToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Brug EmailDataDto direkte
            var emailDataGenerated = new EmailDataDto
            {
                ToEmail = registerDto.Email,
                Subject = "Bekræft din e-mailadresse",
                HtmlMessage = "some message",
            };

            _mockUserAuthService
                .CreateUserAsync(registerDto)
                .Returns(Task.FromResult(identityResultSucceeded));
            _mockUserAuthService
                .FindUserByEmailAsync(registerDto.Email)
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .AddToRoleAsync(user, "User")
                .Returns(Task.FromResult(IdentityResult.Success));
            _mockUserAuthService
                .GenerateEmailConfirmationTokenAsync(user)
                .Returns(Task.FromResult(token));

            _mockEmailService
                .GenerateRegistrationEmailAsync(encodedToken, user)
                .Returns(emailDataGenerated);

            // Brug EmailDataDto direkte i Arg.Any<> og Arg.Is<>
            _mockEmailService
                .When(x => x.SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailDataDto>())) // RETTET
                .Do(callInfo =>
                { /* Gør intet for at undgå reel afsendelse */
                });
            _mockEmailService
                .SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailDataDto>()) // RETTET
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            dynamic value = okResult.Value; // Antager OkObjectResult.Value er et anonymt objekt
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain("Registrering succesfuld!")
            );

            // Verificer at SendEmailAsync blev kaldt med den korrekte EmailDataDto
            await _mockEmailService
                .Received(1)
                .SendEmailAsync(
                    registerDto.Email,
                    Arg.Is<EmailDataDto>(ed => ed.Subject == "Bekræft din e-mailadresse")
                ); // RETTET
        }

        [Test]
        public async Task CreateUser_CreateUserFails_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "invalidPasswor",
            };

            var identityResultFailed = IdentityResult.Failed(
                new IdentityError { Description = "User was not created" }
            );
            
            _mockUserAuthService
                .CreateUserAsync(registerDto)
                .Returns(Task.FromResult(IdentityResult.Failed()));

            // Act
            var result = await _controller.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            dynamic value = badRequestResult.Value;
            Assert.That(value.GetType().GetProperty("errors").GetValue(value, null), Is.Not.Null);
        }

        [Test]
        public async Task CreateUser_AddToRoleFails_ReturnsStatusCode500()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
            };
            var user = new User
            {
                Id = 1,
                UserName = registerDto.Username,
                Email = registerDto.Email,
            };
            var roleResultFailed = IdentityResult.Failed(
                new IdentityError { Description = "Role assignment failed" }
            );

            _mockUserAuthService
                .CreateUserAsync(registerDto)
                .Returns(Task.FromResult(IdentityResult.Success));
            _mockUserAuthService
                .FindUserByEmailAsync(registerDto.Email)
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .AddToRoleAsync(user, "User")
                .Returns(Task.FromResult(roleResultFailed));
            _mockUserAuthService
                .DeleteUserAsync(user)
                .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            dynamic value = objectResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain(
                    "Brugeren blev oprettet, men der opstod en fejl ved tildeling af rolle."
                )
            );
        }

        [Test]
        public async Task VerifyEmail_ValidToken_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var token = "validToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var user = new User { Id = userId, EmailConfirmed = false };

            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .ConfirmEmailAsync(user, token)
                .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.VerifyEmail(userId, encodedToken);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            dynamic value = okResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain("Din emailadresse er bekræftet.")
            );
        }

        [Test]
        public async Task VerifyEmail_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var token = "anyToken"; // encoded form
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.VerifyEmail(userId, token);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Ugyldigt bruger ID."));
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "test@example.com",
                Password = "Password123!",
            };
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var jwtToken = "dummyJwtToken";

            _mockUserAuthService
                .FindUserByEmailAsync(loginDto.EmailOrUsername.ToLower())
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .CheckPasswordSignInAsync(user, loginDto.Password, false)
                .Returns(Task.FromResult(signInResult));
            _mockUserAuthService.GenerateJwtTokenAsync(user).Returns(Task.FromResult(jwtToken));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            dynamic value = okResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("token").GetValue(value, null),
                Is.EqualTo(jwtToken)
            );
        }

        [Test]
        public async Task Login_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "nonexistent@example.com",
                Password = "Password123!",
            };
            _mockUserAuthService
                .FindUserByEmailAsync(loginDto.EmailOrUsername.ToLower())
                .Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Is.EqualTo("Bruger findes ikke")
            );
        }

        [Test]
        public async Task Login_EmailNotConfirmed_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "test@example.com",
                Password = "Password123!",
            };
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                EmailConfirmed = false,
            };

            _mockUserAuthService
                .FindUserByEmailAsync(loginDto.EmailOrUsername.ToLower())
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .CheckPasswordSignInAsync(user, loginDto.Password, false)
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.NotAllowed));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Does.Contain("Din emailadresse er ikke blevet bekræftet.")
            );
        }

        [Test]
        public async Task ForgotPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "nonexistent@example.com" };
            _mockUserAuthService
                .FindUserByEmailAsync(forgotPasswordDto.Email)
                .Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.ResetPassword(forgotPasswordDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Is.EqualTo("Bruger findes ikke.")
            );
        }

        [Test]
        public async Task ResetPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
            };
            var userId = 1;
            var token = "someToken";
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.ResetPassword(resetPasswordDto, userId, token);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Is.EqualTo("Bruger findes ikke.")
            );
        }

        [Test]
        public async Task ResetPassword_PasswordsDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                NewPassword = "NewPassword123!",
                ConfirmPassword = "DifferentPassword123!",
            };
            var userId = 1;
            var token = "someToken";
            var user = new User { Id = userId };
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));

            // Act
            var result = await _controller.ResetPassword(resetPasswordDto, userId, token);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Is.EqualTo("Adgangskoderne skal matche.")
            );
        }

        [Test]
        public void LoginWithGoogle_ValidCall_ReturnsChallengeResult()
        {
            // Arrange
            var clientReturnUrl = "/test-return";
            var sanitizedReturnUrl = "/test-return";
            var expectedPropertiesRedirectUri = "http://localhost/fakeaction";
            var authPropertiesReturnedByService = new AuthenticationProperties
            {
                RedirectUri = expectedPropertiesRedirectUri,
            };

            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(sanitizedReturnUrl);
            _mockUserAuthService.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme, 
                expectedPropertiesRedirectUri) // Den URI controlleren forventes at sende til servicen
                .Returns(authPropertiesReturnedByService);

            // Act
            var result = _controller.LoginWithGoogle(clientReturnUrl);

            // Assert
            Assert.That(result, Is.InstanceOf<ChallengeResult>());
            var challengeResult = result as ChallengeResult;
            Assert.That(challengeResult, Is.Not.Null, "ChallengeResult var null.");
            Assert.That(
                challengeResult.AuthenticationSchemes.Contains(GoogleDefaults.AuthenticationScheme),
                Is.True
            );
            Assert.That(challengeResult.Properties, Is.EqualTo(authPropertiesReturnedByService));
        }
    }
}
