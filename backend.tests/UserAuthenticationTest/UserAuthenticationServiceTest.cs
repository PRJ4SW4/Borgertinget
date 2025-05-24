// Fil: UserAuthenticationServiceTests.cs (Rettet version)

using NUnit.Framework;
using NSubstitute;
using backend.Services.Authentication;
using backend.Repositories.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using backend.Models;
using backend.DTOs;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;

namespace backend.tests.Services
{
    [TestFixture]
    public class UserAuthenticationServiceTests
    {
        private IUserAuthenticationRepository _authenticationRepository;
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private ILogger<UserAuthenticationService> _logger;
        private IConfiguration _config;
        private UserAuthenticationService _service;

        [SetUp]
        public void SetUp()
        {
            var userStore = Substitute.For<IUserStore<User>>();
            _userManager = Substitute.For<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var contextAccessor = Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();
            _signInManager = Substitute.For<SignInManager<User>>(_userManager, contextAccessor, claimsFactory, null, null, null, null);

            _authenticationRepository = Substitute.For<IUserAuthenticationRepository>();
            _logger = Substitute.For<ILogger<UserAuthenticationService>>();
            _config = Substitute.For<IConfiguration>();

            _service = new UserAuthenticationService(
                _authenticationRepository,
                _userManager,
                _signInManager,
                _logger,
                _config
            );
        }

        [TearDown]
        public void TearDown()
        {
            _userManager.Dispose();
        }

        #region CreateUserAsync Tests

        [Test]
        public async Task CreateUserAsync_WithValidPassword_ShouldSucceed()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            _userManager.CreateAsync(Arg.Any<User>(), dto.Password).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.True);
            await _userManager.Received(1).CreateAsync(Arg.Is<User>(u => u.UserName == dto.Username && u.Email == dto.Email), dto.Password);
        }
        
        [Test]
        public async Task CreateUserAsync_WhenUserManagerFails_ShouldReturnFailedResult()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            var identityError = new IdentityError { Description = "Password is too weak." };
            _userManager.CreateAsync(Arg.Any<User>(), dto.Password).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
            Assert.That(result.Errors.First().Description, Is.EqualTo("Password is too weak."));
        }

        #endregion

        #region GenerateJwtTokenAsync Tests

        [Test]
        public async Task GenerateJwtTokenAsync_WithValidUser_ReturnsTokenString()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };
            var userRoles = new List<string> { "User", "Admin" };
            _config["Jwt:Key"].Returns("ThisIsASuperSecretKeyForTesting12345");
            _config["Jwt:Issuer"].Returns("TestIssuer");
            _config["Jwt:Audience"].Returns("TestAudience");
            _userManager.GetRolesAsync(user).Returns(Task.FromResult<IList<string>>(userRoles));

            // Act
            var token = await _service.GenerateJwtTokenAsync(user);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);
        }

        [Test]
        public void GenerateJwtTokenAsync_WhenJwtKeyIsMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };
            _config["Jwt:Key"].Returns((string)null); 

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GenerateJwtTokenAsync(user));
            Assert.That(ex.Message, Is.EqualTo("JWT Key is not configured."));
        }

        #endregion

        #region HandleGoogleLoginCallbackAsync Tests

        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExistingLinkedUser_ReturnsSuccessWithJwt()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };
            var info = CreateFakeExternalLoginInfo("Google", "google-provider-key", "test@example.com");
            _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true).Returns(Task.FromResult(SignInResult.Success));
            _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey).Returns(Task.FromResult(user));
            _config["Jwt:Key"].Returns("ThisIsASuperSecretKeyForTesting12345");
            _userManager.GetRolesAsync(user).Returns(Task.FromResult<IList<string>>(new List<string>()));

            // Act
            var result = await _service.HandleGoogleLoginCallbackAsync(info);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.Success));
            Assert.That(result.JwtToken, Is.Not.Null);
            Assert.That(result.AppUser.Id, Is.EqualTo(user.Id));
            Assert.That(result.ErrorMessage, Is.Null);
        }

        [Test]
        public async Task HandleGoogleLoginCallbackAsync_SignInSucceedsButUserNotFound_ReturnsError()
        {
            // Arrange
            var info = CreateFakeExternalLoginInfo("Google", "google-provider-key", "test@example.com");
            _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true).Returns(Task.FromResult(SignInResult.Success));
            _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _service.HandleGoogleLoginCallbackAsync(info);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.ErrorUserNotFoundAfterSignIn));
            Assert.That(result.ErrorMessage, Is.EqualTo("Bruger konto problem."));
            Assert.That(result.JwtToken, Is.Null);
        }
        
        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExistingUserByEmail_LinksAccountAndReturnsSuccess()
        {
            // Arrange
            var email = "existing@example.com";
            var existingUser = new User { Id = 3, UserName = "existinguser", Email = email, EmailConfirmed = false };
            var info = CreateFakeExternalLoginInfo("Google", "google-provider-key", email);

            _signInManager.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(Task.FromResult(SignInResult.Failed));
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult(existingUser));
            _userManager.UpdateAsync(existingUser).Returns(Task.FromResult(IdentityResult.Success));
            _userManager.AddLoginAsync(existingUser, Arg.Any<UserLoginInfo>()).Returns(Task.FromResult(IdentityResult.Success));
            _userManager.GetRolesAsync(existingUser).Returns(Task.FromResult<IList<string>>(new List<string>()));
            _config["Jwt:Key"].Returns("ThisIsASuperSecretKeyForTesting12345");

            // Act
            var result = await _service.HandleGoogleLoginCallbackAsync(info);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.Success));
            Assert.That(result.JwtToken, Is.Not.Null);
            Assert.That(result.AppUser.EmailConfirmed, Is.True);
            await _userManager.Received(1).UpdateAsync(existingUser);
            await _userManager.Received(1).AddLoginAsync(existingUser, Arg.Any<UserLoginInfo>());
        }

        [Test]
        public async Task HandleGoogleLoginCallbackAsync_LinkLoginFails_ReturnsError()
        {
            // Arrange
            var email = "faillink@example.com";
            var existingUser = new User { Id = 4, UserName = "faillinkuser", Email = email, EmailConfirmed = true };
            var info = CreateFakeExternalLoginInfo("Google", "google-provider-key", email);
            var identityError = new IdentityError { Description = "Login already exists." };

            _signInManager.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(Task.FromResult(SignInResult.Failed));
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult(existingUser));
            _userManager.AddLoginAsync(existingUser, Arg.Any<UserLoginInfo>()).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _service.HandleGoogleLoginCallbackAsync(info);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.ErrorLinkLoginFailed));
            Assert.That(result.ErrorMessage, Is.EqualTo("Kunne ikke linke Google konto."));
        }

        #endregion

        // ... (Hjælpefunktion er den samme) ...
        private ExternalLoginInfo CreateFakeExternalLoginInfo(string loginProvider, string providerKey, string email, string givenName = "Test", string surname = "User")
        {
            var claims = new List<Claim>();
            if (email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }
            claims.Add(new Claim(ClaimTypes.Name, $"{givenName} {surname}"));
            claims.Add(new Claim(ClaimTypes.GivenName, givenName));
            claims.Add(new Claim(ClaimTypes.Surname, surname));

            var claimsIdentity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return new ExternalLoginInfo(claimsPrincipal, loginProvider, providerKey, "Google");
        }
    }
}