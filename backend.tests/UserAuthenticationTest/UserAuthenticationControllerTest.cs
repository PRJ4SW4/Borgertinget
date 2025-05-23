using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using backend.Controllers;
using backend.Services.Authentication;
using backend.DTOs;
using backend.Models;
using NSubstitute; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Text;
using System.Collections.Generic;

namespace Tests.Controllers
{
    [TestFixture]
    public class UsersControllerTests
    {
        private IUserAuthenticationService _mockUserAuthService;
        private EmailService _mockEmailService; 
        private IConfiguration _mockConfiguration;
        private ILogger<UsersController> _mockLogger;
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserAuthService = Substitute.For<IUserAuthenticationService>();
            _mockConfiguration = Substitute.For<IConfiguration>();

            // For EmailService, hvis dens metoder er virtuelle, kan de substitutes.
            // Ellers mock dens dependencies (IConfiguration, ILogger<EmailService>)
            // og opret en reel instans med de mockede dependencies, eller brug en reel instans hvis den er simpel.
            // Her antager vi, at vi kan substitute de metoder, vi kalder på EmailService, eller at den ikke har kompleks intern logik, der kræver dyb mocking.
            // Hvis EmailService ikke har en interface og dens metoder ikke er virtuelle, kan du ikke direkte substitute dens opførsel.
            // I så fald ville du teste EmailService som en del af controller-testen, eller refaktorere EmailService til at bruge en interface.
            // For nu antager vi, at vi kan opsætte dens adfærd eller at dens simple logik er okay at inkludere.
            // For at gøre det mere testbart, ville en IEmailService være bedre.
            // Da EmailService er konkret og ikke har en interface i din kode, mockes dens afhængigheder her:
            var mockEmailServiceLogger = Substitute.For<ILogger<EmailService>>();
            _mockEmailService = Substitute.ForPartsOf<EmailService>(_mockConfiguration, mockEmailServiceLogger);


            _mockLogger = Substitute.For<ILogger<UsersController>>();

            _controller = new UsersController(
                _mockConfiguration,
                _mockEmailService,
                _mockLogger,
                _mockUserAuthService
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
            }, "mock"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Test]
        public async Task CreateUser_ValidDto_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterUserDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            var identityResultSucceeded = IdentityResult.Success;
            var user = new User { Id = 1, UserName = registerDto.Username, Email = registerDto.Email };
            var token = "dummyToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var emailData = new EmailService.EmailData();

            _mockUserAuthService.CreateUserAsync(registerDto).Returns(Task.FromResult(identityResultSucceeded));
            _mockUserAuthService.FindUserByEmailAsync(registerDto.Email).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService.AddToRoleAsync(user, "User").Returns(Task.FromResult(IdentityResult.Success));
            _mockUserAuthService.GenerateEmailConfirmationTokenAsync(user).Returns(Task.FromResult(token));
            // For konkret EmailService, kan vi ikke direkte substitute interne kald, kun dem vi selv laver.
            // Hvis GenerateRegistrationEmailAsync var på en IEmailService, kunne vi gøre:
            // _mockEmailService.GenerateRegistrationEmailAsync(encodedToken, user).Returns(emailData);
            // Da den er konkret, og vi har mock'et dens dependencies, vil den reelle metode blive kaldt.
            // Vi kan dog "substitute" SendEmailAsync hvis den er virtuel. Hvis ikke, kan vi ikke.
            // Lad os antage SendEmailAsync er virtuel for testens skyld, eller at vi er okay med at den kaldes.
            _mockEmailService.When(x => x.SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailService.EmailData>()))
                             .DoNotCallBase(); // Forhindrer reel afsendelse
            _mockEmailService.SendEmailAsync(registerDto.Email, Arg.Is<EmailService.EmailData>(ed => ed.ToEmail == registerDto.Email))
                             .Returns(Task.CompletedTask); // Opsæt return værdi

            // Vi kalder den rigtige GenerateRegistrationEmailAsync, da den ikke kan substitutes direkte på en konkret klasse uden interface/virtual
            // og vi tjekker resultatet af controllerens kald til den.
            _mockEmailService.GenerateRegistrationEmailAsync(encodedToken, user).Returns(new EmailService.EmailData { ToEmail = registerDto.Email, Subject = "Bekræft din e-mailadresse", HtmlMessage = "some message" });


            // Act
            var result = await _controller.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            dynamic value = okResult.Value;
            Assert.That((string)value.GetType().GetProperty("message").GetValue(value, null), Does.Contain("Registrering succesfuld!"));
            await _mockEmailService.Received(1).SendEmailAsync(registerDto.Email, Arg.Any<EmailService.EmailData>());
        }

        [Test]
        public async Task CreateUser_CreateUserFails_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterUserDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            var identityResultFailed = IdentityResult.Failed(new IdentityError { Description = "Creation failed" });

            _mockUserAuthService.CreateUserAsync(registerDto).Returns(Task.FromResult(identityResultFailed));

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
            var registerDto = new RegisterUserDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            var user = new User { Id = 1, UserName = registerDto.Username, Email = registerDto.Email };
            var roleResultFailed = IdentityResult.Failed(new IdentityError { Description = "Role assignment failed" });

            _mockUserAuthService.CreateUserAsync(registerDto).Returns(Task.FromResult(IdentityResult.Success));
            _mockUserAuthService.FindUserByEmailAsync(registerDto.Email).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService.AddToRoleAsync(user, "User").Returns(Task.FromResult(roleResultFailed));
            _mockUserAuthService.DeleteUserAsync(user).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            dynamic value = objectResult.Value;
            Assert.That((string)value.GetType().GetProperty("message").GetValue(value, null), Does.Contain("Brugeren blev oprettet, men der opstod en fejl ved tildeling af rolle."));
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
            _mockUserAuthService.ConfirmEmailAsync(user, token).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.VerifyEmail(userId, encodedToken);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            dynamic value = okResult.Value;
            Assert.That((string)value.GetType().GetProperty("message").GetValue(value, null), Does.Contain("Din emailadresse er bekræftet."));
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
            var loginDto = new LoginDto { EmailOrUsername = "test@example.com", Password = "Password123!" };
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var jwtToken = "dummyJwtToken";

            _mockUserAuthService.FindUserByEmailAsync(loginDto.EmailOrUsername.ToLower()).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService.CheckPasswordSignInAsync(user, loginDto.Password, false).Returns(Task.FromResult(signInResult));
            _mockUserAuthService.GenerateJwtTokenAsync(user).Returns(Task.FromResult(jwtToken));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            dynamic value = okResult.Value;
            Assert.That((string)value.GetType().GetProperty("token").GetValue(value, null), Is.EqualTo(jwtToken));
        }

        [Test]
        public async Task Login_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto { EmailOrUsername = "nonexistent@example.com", Password = "Password123!" };
            _mockUserAuthService.FindUserByEmailAsync(loginDto.EmailOrUsername.ToLower()).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That((string)value.GetType().GetProperty("error").GetValue(value, null), Is.EqualTo("Bruger findes ikke"));
        }

        [Test]
        public async Task Login_EmailNotConfirmed_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto { EmailOrUsername = "test@example.com", Password = "Password123!" };
            var user = new User { UserName = "testuser", Email = "test@example.com", EmailConfirmed = false };

            _mockUserAuthService.FindUserByEmailAsync(loginDto.EmailOrUsername.ToLower()).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService.CheckPasswordSignInAsync(user, loginDto.Password, false).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.NotAllowed));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That((string)value.GetType().GetProperty("error").GetValue(value, null), Does.Contain("Din emailadresse er ikke blevet bekræftet."));
        }

        [Test]
        public async Task ForgotPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "nonexistent@example.com" };
            _mockUserAuthService.FindUserByEmailAsync(forgotPasswordDto.Email).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.ResetPassword(forgotPasswordDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That((string)value.GetType().GetProperty("error").GetValue(value, null), Is.EqualTo("Bruger findes ikke."));
        }

        [Test]
        public async Task ResetPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto { NewPassword = "NewPassword123!", ConfirmPassword = "NewPassword123!" };
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
            Assert.That((string)value.GetType().GetProperty("error").GetValue(value, null), Is.EqualTo("Bruger findes ikke."));
        }

        [Test]
        public async Task ResetPassword_PasswordsDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto { NewPassword = "NewPassword123!", ConfirmPassword = "DifferentPassword123!" };
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
            Assert.That((string)value.GetType().GetProperty("error").GetValue(value, null), Is.EqualTo("Adgangskoderne skal matche."));
        }

         [Test]
        public void LoginWithGoogle_ValidCall_ReturnsChallengeResult()
        {
            // Arrange
            var clientReturnUrl = "/test-return";
            var sanitizedReturnUrl = "/test-return";
            var propertiesRedirectUri = "http://localhost/api/Users/HandleGoogleCallback?returnUrl=%2Ftest-return";
            var authProperties = new AuthenticationProperties { RedirectUri = propertiesRedirectUri }; 

            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(sanitizedReturnUrl);
            _mockUserAuthService.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, Arg.Is<string>(s => s.Contains(clientReturnUrl))).Returns(authProperties); 

            // Act
            var result = _controller.LoginWithGoogle(clientReturnUrl);

            // Assert
            Assert.That(result, Is.InstanceOf<ChallengeResult>());
            var challengeResult = result as ChallengeResult;
            Assert.That(challengeResult, Is.Not.Null);
            Assert.That(challengeResult.AuthenticationSchemes.Contains(GoogleDefaults.AuthenticationScheme), Is.True); 
            Assert.That(challengeResult.Properties, Is.EqualTo(authProperties));
        }
    }
}