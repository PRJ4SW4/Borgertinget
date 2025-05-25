using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTO.UserAuthentication;
using backend.DTOs;
using backend.Models;
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
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;
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
        private UsersController _uut;
        private IUrlHelper _mockUrlHelper;

        [SetUp]
        public void Setup()
        {
            _mockUserAuthService = Substitute.For<IUserAuthenticationService>();
            _mockConfiguration = Substitute.For<IConfiguration>();
            var mockEmailServiceLogger = Substitute.For<ILogger<EmailService>>();
            _mockEmailService = Substitute.For<IEmailService>();
            _mockLogger = Substitute.For<ILogger<UsersController>>();
            _mockUrlHelper = Substitute.For<IUrlHelper>();

            _mockConfiguration["FrontendBaseUrl"].Returns("http://localhost:5173");

            _uut = new UsersController(
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

            var defaultHttpContext = new DefaultHttpContext() { User = user };
            defaultHttpContext.Request.Scheme = "http"; 

            // Opsætning for HttpContext.SignOutAsync(IdentityConstants.ExternalScheme)
            var authServiceMock = Substitute.For<IAuthenticationService>();
            var services = new ServiceCollection();
            services.AddSingleton(authServiceMock); // Gør IAuthenticationService tilgængelig
            defaultHttpContext.RequestServices = services.BuildServiceProvider();

            authServiceMock
            .SignOutAsync(Arg.Any<HttpContext>(), IdentityConstants.ExternalScheme, Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask); // Sikrer at kaldet ikke fejler


            _uut.ControllerContext = new ControllerContext()
            {
                HttpContext = defaultHttpContext,
            };
            _uut.Url = _mockUrlHelper;
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

            _mockEmailService
                .When(x => x.SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailDataDto>()))
                .Do(callInfo =>
                { });
            _mockEmailService
                .SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailDataDto>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _uut.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            dynamic value = okResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain("Registrering succesfuld! Tjek din email for at bekræfte din konto.")
            );


            // await _mockEmailService
            //     .Received(1)
            //     .SendEmailAsync(
            //         registerDto.Email,
            //         Arg.Is<EmailDataDto>(ed => ed.Subject == "Bekræft din e-mailadresse")
            //     ); 
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
            var result = await _uut.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            dynamic value = badRequestResult.Value;
            Assert.That(value.GetType().GetProperty("errors").GetValue(value, null), Is.Not.Null);
        }

        [Test]
        public async Task CreateUser_UserNotFoundAfterCreation_ReturnsStatusCode500()
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

            _mockUserAuthService
                .CreateUserAsync(registerDto)
                .Returns(Task.FromResult(identityResultSucceeded));
            _mockUserAuthService
                .FindUserByEmailAsync(registerDto.Email)
                .Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _uut.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(400));
            dynamic value = objectResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Does.Contain("Bruger blev ikke oprettet.")
            );
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
            var result = await _uut.CreateUser(registerDto);

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
        public async Task CreateUser_EmailSendingFails_ReturnsStatusCode500()
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
            var token = "dummyToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var emailDataGenerated = new EmailDataDto
            {
                ToEmail = registerDto.Email,
                Subject = "Bekræft din e-mailadresse",
                HtmlMessage = "some message",
            };

            _mockUserAuthService
                .CreateUserAsync(registerDto)
                .Returns(Task.FromResult(IdentityResult.Success));
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

            // Simulate email sending failure by throwing an exception
            _mockEmailService
                .SendEmailAsync(Arg.Any<string>(), Arg.Any<EmailDataDto>())
                .Throws(new Exception("Simulated email service error."));

            // Act
            var result = await _uut.CreateUser(registerDto);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(500)); // Expect 500 Internal Server Error

            dynamic value = objectResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.That(messageProperty, Is.Not.Null);
            Assert.That(
                (string)messageProperty.GetValue(value, null),
                Does.Contain("Fejl ved afsendelse af mail. Prøv venligst igen senere.")
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
            var result = await _uut.VerifyEmail(userId, encodedToken);

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
        public async Task VerifyEmail_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var token = ""; // Invalid token

            // Act
            var result = await _uut.VerifyEmail(userId, token);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Token mangler."));
        }

        [Test]
        public async Task VerifyEmail_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var token = "anyToken";
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _uut.VerifyEmail(userId, token);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Ugyldigt bruger ID."));
        }

        [Test]
        public async Task VerifyEmail_VerificationFailed_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            string decodedToken = "decodedTokenContent";

            var user = new User { Id = userId, EmailConfirmed = false };
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .ConfirmEmailAsync(user, decodedToken)
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Verification failed" })));

            var tokenForController = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(decodedToken));

            // Act
            var result = await _uut.VerifyEmail(userId, tokenForController);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(
                badRequestResult.Value as string,
                Does.Contain("Ugyldigt eller udløbet verifikationslink")
            );
        }

        [Test]
        public async Task VerifyEmail_ExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var decodedToken = "invalidToken";

            var user = new User { Id = userId, EmailConfirmed = false };
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .ConfirmEmailAsync(user, decodedToken)
                .Throws(new Exception("Simulated email service error."));

            var tokenForController = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(decodedToken));

            // Act
            var result = await _uut.VerifyEmail(userId, tokenForController);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            dynamic value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.That(messageProperty, Is.Not.Null);
            Assert.That(
                (string)messageProperty.GetValue(value, null),
                Does.Contain("Ugyldigt token format")
            );
        }

        [Test]
        public async Task Login_ValidCredentialsWithEmail_ReturnsOkWithToken()
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
            var result = await _uut.Login(loginDto);

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
        public async Task Login_ValidCredentialsWithUsername_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "testuser",
                Password = "Password123!",
            };
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var jwtToken = "dummyJwtToken";

            _mockUserAuthService
                .FindUserByNameAsync(loginDto.EmailOrUsername.ToLower())
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .CheckPasswordSignInAsync(user, loginDto.Password, false)
                .Returns(Task.FromResult(signInResult));
            _mockUserAuthService.GenerateJwtTokenAsync(user).Returns(Task.FromResult(jwtToken));

            // Act
            var result = await _uut.Login(loginDto);

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
            var result = await _uut.Login(loginDto);

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
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "testuser",
                Password = "WrongPassword!",
            };

            var user = new User { UserName = "testuser", Email = "testuser@email.com" };

            _mockUserAuthService
                .FindUserByNameAsync(loginDto.EmailOrUsername.ToLower())
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .CheckPasswordSignInAsync(user, loginDto.Password, false)
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            // Act
            var result = await _uut.Login(loginDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Does.Contain("Forkert adgangskode")
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
            var result = await _uut.Login(loginDto);

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
            var result = await _uut.ResetPassword(forgotPasswordDto);

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
        public async Task ForgotPassword_ValidEmail_ReturnsOk()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "test@email.com" };
            var user = new User { Id = 1, Email = forgotPasswordDto.Email };

            _mockUserAuthService
                .FindUserByEmailAsync(forgotPasswordDto.Email)
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .GeneratePasswordResetTokenAsync(user)
                .Returns(Task.FromResult("dummyToken"));
            _mockEmailService
                .GenerateResetPasswordEmailAsync("dummyToken", user)
                .Returns(new EmailDataDto
                {
                    ToEmail = user.Email,
                    Subject = "Nulstil din adgangskode",
                    HtmlMessage = "some message",
                });
            _mockEmailService
                .SendEmailAsync(user.Email, Arg.Any<EmailDataDto>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _uut.ResetPassword(forgotPasswordDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            dynamic value = okResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain("En mail med et link til at nulstille din adgangskode er blevet sendt.")
            );
        }

        [Test]
        public async Task ForgotPassword_ValidEmail_EmailSendingFails_ThrowException()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "test@email.com" };
            var user = new User { Id = 1, Email = forgotPasswordDto.Email };

            _mockUserAuthService
                .FindUserByEmailAsync(forgotPasswordDto.Email)
                .Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .GeneratePasswordResetTokenAsync(user)
                .Returns(Task.FromResult("dummyToken"));
            _mockEmailService
                .GenerateResetPasswordEmailAsync("dummyToken", user)
                .Returns(new EmailDataDto
                {
                    ToEmail = user.Email,
                    Subject = "Nulstil din adgangskode",
                    HtmlMessage = "some message",
                });
            _mockEmailService
                .SendEmailAsync(user.Email, Arg.Any<EmailDataDto>())
                .Throws(new Exception("Simulated email service error."));

            // Act
            var result = await _uut.ResetPassword(forgotPasswordDto);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            dynamic value = objectResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain("Fejl ved afsendelse af nulstillingsmail. Prøv venligst igen senere.")
            );
        }

        [Test]
        public async Task ResetPassword_ValidRequest_ReturnsOk()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
            };
            var userId = 1;
            var token = "validToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var user = new User { Id = userId, EmailConfirmed = true };

            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .ResetPasswordAsync(user, token, resetPasswordDto.NewPassword)
                .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.ResetPassword(resetPasswordDto, userId, encodedToken);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            dynamic value = okResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Is.EqualTo("Adgangskoden er blevet ændret.")
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
            var result = await _uut.ResetPassword(resetPasswordDto, userId, token);

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
            var result = await _uut.ResetPassword(resetPasswordDto, userId, token);

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
        public async Task ResetPassword_PasswordIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                NewPassword = "invalid",
                ConfirmPassword = "invalid",
            };
            var userId = 1;
            var token = "someToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var user = new User { Id = userId };
            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));

            _mockUserAuthService
                .ResetPasswordAsync(user, token, resetPasswordDto.NewPassword)
                .Returns(Task.FromResult(IdentityResult.Failed()));

            // Act
            var result = await _uut.ResetPassword(resetPasswordDto, userId, encodedToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            dynamic value = badRequestResult.Value;
            Assert.That(value.GetType().GetProperty("errors").GetValue(value, null), Is.Not.Null);
        }

        [Test]
        public async Task ResetPassword_ValidPassword_ResetFails_ThrowException()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!",
            };
            var userId = 1;
            var token = "validToken";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var user = new User { Id = userId, EmailConfirmed = true };

            _mockUserAuthService.GetUserAsync(userId).Returns(Task.FromResult<User?>(user));
            _mockUserAuthService
                .ResetPasswordAsync(user, token, resetPasswordDto.NewPassword)
                .Throws(new Exception("Simulated reset password error."));

            // Act
            var result = await _uut.ResetPassword(resetPasswordDto, userId, encodedToken);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(400));
            dynamic value = objectResult.Value;
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Does.Contain("Ugyldigt token format")
            );
        }
        #region OAuth del
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

            _mockUrlHelper.Action(Arg.Is<UrlActionContext>(uac =>
                uac.Action == nameof(UsersController.HandleGoogleCallback) &&
                uac.Controller == "Users" &&
                (string?)uac.Values.GetType().GetProperty("returnUrl").GetValue(uac.Values, null) == clientReturnUrl &&
                uac.Protocol == "http" 
            )).Returns(expectedPropertiesRedirectUri);

            _mockUserAuthService.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                expectedPropertiesRedirectUri)
                .Returns(authPropertiesReturnedByService);

            // Act
            var result = _uut.LoginWithGoogle(clientReturnUrl);

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
        
        [Test]
        public void LoginWithGoogle_WhenUrlActionReturnsNull_ReturnsStatusCode500()
        {
            // Arrange
            var clientReturnUrl = "/test-return";
            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(clientReturnUrl);

            _mockUrlHelper.Action(Arg.Any<UrlActionContext>()).Returns((string)null);

            // Act
            var result = _uut.LoginWithGoogle(clientReturnUrl);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(objectResult.Value, Is.EqualTo("Intern fejl: Kunne ikke starte Google login."));
        }

        [Test]
        public void LoginWithGoogle_WhenClientReturnUrlIsNull_CallsSanitizeAndProceeds()
        {
            // Arrange
            string? clientReturnUrl = null;
            var sanitizedReturnUrl = "/"; // Dette er hvad SanitizeReturnUrl forventes at returnere for null
            var expectedPropertiesRedirectUri = "http://localhost/someaction"; // En gyldig URL
            var authPropertiesReturnedByService = new AuthenticationProperties { RedirectUri = expectedPropertiesRedirectUri };

            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(sanitizedReturnUrl);

            _mockUrlHelper.Action(Arg.Is<UrlActionContext>(uac =>
                uac.Action == nameof(UsersController.HandleGoogleCallback) &&
                uac.Controller == "Users" &&
                ((string?)uac.Values.GetType().GetProperty("returnUrl").GetValue(uac.Values, null)) == clientReturnUrl && // clientReturnUrl er null her
                uac.Protocol == "http"
            )).Returns(expectedPropertiesRedirectUri);

            _mockUserAuthService.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                expectedPropertiesRedirectUri)
                .Returns(authPropertiesReturnedByService);

            // Act
            var result = _uut.LoginWithGoogle(clientReturnUrl);

            // Assert
            _mockUserAuthService.Received(1).SanitizeReturnUrl(null);
            Assert.That(result, Is.InstanceOf<ChallengeResult>());
        }


        // --- Tests for HandleGoogleCallback ---

        [Test]
        public async Task HandleGoogleCallback_WithRemoteError_RedirectsToLoginWithError()
        {
            // Arrange
            var remoteError = "google_auth_error";
            var expectedRedirectUrl = $"http://localhost:5173/login?error={HttpUtility.UrlEncode(remoteError)}";

            // Act
            var result = await _uut.HandleGoogleCallback(null, remoteError);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }

        [Test]
        public async Task HandleGoogleCallback_GetExternalLoginInfoReturnsNull_RedirectsToLoginWithError()
        {
            // Arrange
            _mockUserAuthService.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(null));
            var expectedRedirectUrl = $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Fejl ved eksternt login.")}";

            // Act
            var result = await _uut.HandleGoogleCallback(null, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }

        [Test]
        public async Task HandleGoogleCallback_ServiceReturnsError_RedirectsToLoginWithError()
        {
            // Arrange
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "Google", "providerKey", "Google");
            _mockUserAuthService.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(externalLoginInfo));

            var serviceErrorResult = new GoogleLoginResultDto
            {
                Status = GoogleLoginStatus.ErrorCreateUserFailed,
                ErrorMessage = "Kunne ikke oprette bruger."
            };
            _mockUserAuthService.HandleGoogleLoginCallbackAsync(externalLoginInfo)
                                .Returns(Task.FromResult(serviceErrorResult));

            var expectedRedirectUrl = $"http://localhost:5173/login?error={HttpUtility.UrlEncode(serviceErrorResult.ErrorMessage)}";

            // Act
            var result = await _uut.HandleGoogleCallback(null, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }

        [Test]
        public async Task HandleGoogleCallback_ServiceReturnsErrorWithoutMessage_RedirectsToLoginWithGenericError()
        {
            // Arrange
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "Google", "providerKey", "Google");
            _mockUserAuthService.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(externalLoginInfo));

            var serviceErrorResult = new GoogleLoginResultDto
            {
                Status = GoogleLoginStatus.ErrorNoLoginInfo, 
                ErrorMessage = null 
            };
            _mockUserAuthService.HandleGoogleLoginCallbackAsync(externalLoginInfo)
                                .Returns(Task.FromResult(serviceErrorResult));

            var expectedRedirectUrl = $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Ukendt fejl ved Google login.")}";

            // Act
            var result = await _uut.HandleGoogleCallback(null, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }

        [Test]
        public async Task HandleGoogleCallback_Success_RedirectsToLoginSuccessWithTokenAndReturnUrl()
        {
            // Arrange
            var clientReturnUrl = "/original-path";
            var sanitizedClientReturnUrl = "/original-path";
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "Google", "providerKey", "Google");
            var appUser = new User { Id = 1, UserName = "googleuser" };
            var jwtToken = "generated.jwt.token";

            _mockUserAuthService.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(externalLoginInfo));
            _mockUserAuthService.HandleGoogleLoginCallbackAsync(externalLoginInfo)
                                .Returns(Task.FromResult(new GoogleLoginResultDto
                                {
                                    Status = GoogleLoginStatus.Success,
                                    JwtToken = jwtToken,
                                    AppUser = appUser
                                }));
            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(sanitizedClientReturnUrl);
            _mockUserAuthService.SignOutAsync().Returns(Task.CompletedTask); // Mock din service SignOut

            var expectedRedirectUrl = $"http://localhost:5173/login-success?token={jwtToken}&originalReturnUrl={HttpUtility.UrlEncode(sanitizedClientReturnUrl).Replace("%2f", "%2F")}";

            // Act
            var result = await _uut.HandleGoogleCallback(clientReturnUrl, null);

            // Assert
            await _mockUserAuthService.Received(1).SignOutAsync(); 

            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }

        [Test]
        public async Task HandleGoogleCallback_Success_WhenReturnUrlIsNull_RedirectsToLoginSuccessWithTokenOnly()
        {
            // Arrange
            string? clientReturnUrl = null;
            var sanitizedClientReturnUrl = "/";
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "Google", "providerKey", "Google");
            var appUser = new User { Id = 1, UserName = "googleuser" };
            var jwtToken = "generated.jwt.token";

            _mockUserAuthService.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(externalLoginInfo));
            _mockUserAuthService.HandleGoogleLoginCallbackAsync(externalLoginInfo)
                                .Returns(Task.FromResult(new GoogleLoginResultDto
                                {
                                    Status = GoogleLoginStatus.Success,
                                    JwtToken = jwtToken,
                                    AppUser = appUser
                                }));
            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(sanitizedClientReturnUrl);
            _mockUserAuthService.SignOutAsync().Returns(Task.CompletedTask);

            var expectedRedirectUrl = $"http://localhost:5173/login-success?token={jwtToken}&originalReturnUrl=%2F";

            // Act
            var result = await _uut.HandleGoogleCallback(clientReturnUrl, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }

        [Test]
        public async Task HandleGoogleCallback_Success_WhenReturnUrlIsInvalid_RedirectsToLoginSuccessWithTokenOnly()
        {
            // Arrange
            var clientReturnUrl = "http://malicious.com/path"; // Ugyldig
            var sanitizedClientReturnUrl = "http://malicious.com/path"; // Antager SanitizeReturnUrl ikke ændrer den, men controller-logik ignorerer den
            var externalLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "Google", "providerKey", "Google");
            var appUser = new User { Id = 1, UserName = "googleuser" };
            var jwtToken = "generated.jwt.token";

            _mockUserAuthService.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(externalLoginInfo));
            _mockUserAuthService.HandleGoogleLoginCallbackAsync(externalLoginInfo)
                                .Returns(Task.FromResult(new GoogleLoginResultDto
                                {
                                    Status = GoogleLoginStatus.Success,
                                    JwtToken = jwtToken,
                                    AppUser = appUser
                                }));
            _mockUserAuthService.SanitizeReturnUrl(clientReturnUrl).Returns(sanitizedClientReturnUrl);
            _mockUserAuthService.SignOutAsync().Returns(Task.CompletedTask);


            var expectedRedirectUrl = $"http://localhost:5173/login-success?token={jwtToken}";

            // Act
            var result = await _uut.HandleGoogleCallback(clientReturnUrl, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo(expectedRedirectUrl));
        }
        
#endregion
    }
}
