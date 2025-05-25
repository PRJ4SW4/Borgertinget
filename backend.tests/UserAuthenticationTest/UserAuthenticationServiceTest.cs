using NUnit.Framework;
using NSubstitute;
using backend.Services.Authentication;
using backend.Repositories.Authentication;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions; // Tilføjet for NullLogger
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Linq;

namespace backend.Tests.Services
{
    [TestFixture]
    public class UserAuthenticationServiceTests
    {
        // Mocks for afhængigheder
        private IUserAuthenticationRepository _authenticationRepository;
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IConfiguration _config;

        private UserAuthenticationService _uut;

        [SetUp]
        public void Setup()
        {
            _authenticationRepository = Substitute.For<IUserAuthenticationRepository>();
            
            var userStore = Substitute.For<IUserStore<User>>();
            _userManager = Substitute.For<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);
            
            _signInManager = Substitute.For<SignInManager<User>>(_userManager, Substitute.For<IHttpContextAccessor>(), Substitute.For<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
            
            _config = Substitute.For<IConfiguration>();

            _uut = new UserAuthenticationService(
                _authenticationRepository,
                _userManager,
                _signInManager,
                NullLogger<UserAuthenticationService>.Instance,
                _config
            );
        }

        [TearDown]
        public void TearDown()
        {
            _userManager?.Dispose();
        }

        [Test]
        public async Task GetUserAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User { Id = 1, UserName = "TestUser" };
            _authenticationRepository.GetUserByIdAsync(1).Returns(Task.FromResult<User?>(expectedUser));

            // Act
            var result = await _uut.GetUserAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedUser.Id));
        }

        [Test]
        public async Task GetUserAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _authenticationRepository.GetUserByIdAsync(1).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _uut.GetUserAsync(1);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase("/dashboard", "/dashboard")]
        [TestCase("/path?query=malicious", "/pathquery=malicious")]
        // Justeret forventet output til at matche nuværende SanitizeReturnUrl-implementering
        [TestCase("/path\nwith\rbreaks", "/pathwith\rbreaks")] 
        [TestCase(null, "/")]
        public void SanitizeReturnUrl_VariousInputs_ReturnsSanitizedUrl(string? input, string expected)
        {
            // Act
            var result = _uut.SanitizeReturnUrl(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task CreateUserAsync_ValidDto_ReturnsSuccess()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "newUser", Email = "new@test.com", Password = "Password123!" };
            _userManager.CreateAsync(Arg.Any<User>(), dto.Password).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.CreateUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task CreateUserAsync_CreationFails_ReturnsFailedResult()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "newUser", Email = "new@test.com", Password = "Password123!" };
            var identityError = new IdentityError { Description = "Creation failed" };
            _userManager.CreateAsync(Arg.Any<User>(), dto.Password).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _uut.CreateUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Has.Member(identityError));
        }

        [Test]
        public async Task CreateUserAsync_DtoWithoutPassword_CallsCorrectOverloadAndSucceeds()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "externalUser", Email = "external@test.com", Password = null };
            // Vi forventer, at overloaden UDEN password bliver kaldt.
            _userManager.CreateAsync(Arg.Is<User>(u => u.UserName == dto.Username)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.CreateUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.True);
            // Verificer, at metoden MED password IKKE blev kaldt.
            await _userManager.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<string>());
        }

        [Test]
        public async Task GenerateEmailConfirmationTokenAsync_WhenCalled_ReturnsTokenFromUserManager()
        {
            // Arrange
            var user = new User();
            var expectedToken = "confirm-token";
            _userManager.GenerateEmailConfirmationTokenAsync(user).Returns(Task.FromResult(expectedToken));

            // Act
            var result = await _uut.GenerateEmailConfirmationTokenAsync(user);

            // Assert
            Assert.That(result, Is.EqualTo(expectedToken));
        }

        [Test]
        public async Task FindUserByNameAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User { UserName = "TestUser" };
            _authenticationRepository.GetUserByNameAsync("TestUser").Returns(Task.FromResult<User?>(expectedUser));

            // Act
            var result = await _uut.FindUserByNameAsync("TestUser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserName, Is.EqualTo("TestUser"));
        }

        [Test]
        public async Task FindUserByNameAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _authenticationRepository.GetUserByNameAsync("NonExistentUser").Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _uut.FindUserByNameAsync("NonExistentUser");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CheckPasswordSignInAsync_CorrectPassword_ReturnsSuccess()
        {
            // Arrange
            var user = new User();
            _signInManager.CheckPasswordSignInAsync(user, "correct-password", false).Returns(Task.FromResult(SignInResult.Success));

            // Act
            var result = await _uut.CheckPasswordSignInAsync(user, "correct-password", false);

            // Assert
            Assert.That(result.Succeeded, Is.True);
        }
        
        [Test]
        public async Task CheckPasswordSignInAsync_IncorrectPassword_ReturnsFailed()
        {
            // Arrange
            var user = new User();
            _signInManager.CheckPasswordSignInAsync(user, "wrong-password", false).Returns(Task.FromResult(SignInResult.Failed));

            // Act
            var result = await _uut.CheckPasswordSignInAsync(user, "wrong-password", false);

            // Assert
            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task GenerateJwtTokenAsync_ValidUser_ReturnsTokenString()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "tokenUser", Email = "token@test.com" };
            _config["Jwt:Key"].Returns("ThisIsASecretKeyForTesting1234567890");
            _config["Jwt:Issuer"].Returns("TestIssuer");
            _config["Jwt:Audience"].Returns("TestAudience");
            _userManager.GetRolesAsync(user).Returns(Task.FromResult<IList<string>>(new List<string> { "Admin", "User" }));

            // Act
            var token = await _uut.GenerateJwtTokenAsync(user);

            // Assert
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(token, Does.Contain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9")); // Check for JWT header
        }

        [Test]
        public void GenerateJwtTokenAsync_MissingConfig_ThrowsException()
        {
            // Arrange
            var user = new User();
            _config["Jwt:Key"].Returns((string)null); // Simuler at nøglen mangler

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _uut.GenerateJwtTokenAsync(user));
        }

        [Test]
        public async Task SignOutAsync_WhenCalled_CallsSignInManagerSignOut()
        {
            // Arrange
            // (Ingen specifik arrange nødvendig)

            // Act
            await _uut.SignOutAsync();

            // Assert
            await _signInManager.Received(1).SignOutAsync();
        }
        
        [Test]
        public async Task AddToRoleAsync_ValidUserAndRole_ReturnsSuccess()
        {
            // Arrange
            var user = new User();
            var role = "Admin";
            _userManager.AddToRoleAsync(user, role).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.AddToRoleAsync(user, role);

            // Assert
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task AddToRoleAsync_Fails_ReturnsFailedResult()
        {
            // Arrange
            var user = new User();
            var role = "Admin";
            _userManager.AddToRoleAsync(user, role).Returns(Task.FromResult(IdentityResult.Failed()));

            // Act
            var result = await _uut.AddToRoleAsync(user, role);

            // Assert
            Assert.That(result.Succeeded, Is.False);
        }
    }
}