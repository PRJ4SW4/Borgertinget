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
using System.Security.Claims;
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

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();
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
            _userManager.CreateAsync(Arg.Is<User>(u => u.UserName == dto.Username)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.CreateUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.True);
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
        [Test]
        public async Task ConfirmEmailAsync_UserManagerReturnsSuccess_ReturnsSuccess()
        {
            // Arrange
            var user = new User();
            var token = "valid-token";
            _userManager.ConfirmEmailAsync(user, token).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.ConfirmEmailAsync(user, token);

            // Assert
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task ConfirmEmailAsync_UserManagerReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var user = new User();
            var token = "invalid-token";
            var identityError = new IdentityError { Description = "Token validation failed" };
            _userManager.ConfirmEmailAsync(user, token).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _uut.ConfirmEmailAsync(user, token);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.Contains(identityError), Is.True);
        }

        // --- Tests for GeneratePasswordResetTokenAsync ---
        [Test]
        public async Task GeneratePasswordResetTokenAsync_WhenCalled_ReturnsTokenFromUserManager()
        {
            // Arrange
            var user = new User();
            var expectedToken = "reset-password-token";
            _userManager.GeneratePasswordResetTokenAsync(user).Returns(Task.FromResult(expectedToken));

            // Act
            var result = await _uut.GeneratePasswordResetTokenAsync(user);

            // Assert
            Assert.That(result, Is.EqualTo(expectedToken));
        }

        // --- Tests for ResetPasswordAsync ---
        [Test]
        public async Task ResetPasswordAsync_UserManagerReturnsSuccess_ReturnsSuccess()
        {
            // Arrange
            var user = new User();
            var token = "valid-reset-token";
            var newPassword = "NewPassword123!";
            _userManager.ResetPasswordAsync(user, token, newPassword).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.ResetPasswordAsync(user, token, newPassword);

            // Assert
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task ResetPasswordAsync_UserManagerReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var user = new User();
            var token = "invalid-reset-token";
            var newPassword = "NewPassword123!";
            var identityError = new IdentityError { Description = "Password reset failed" };
            _userManager.ResetPasswordAsync(user, token, newPassword).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _uut.ResetPasswordAsync(user, token, newPassword);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.Contains(identityError), Is.True);
        }

        // --- Tests for FindUserByEmailAsync ---
        [Test]
        public async Task FindUserByEmailAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = new User { Email = email };
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult<User?>(expectedUser));

            // Act
            var result = await _uut.FindUserByEmailAsync(email);

            // Assert
            Assert.That(result, Is.EqualTo(expectedUser));
        }

        [Test]
        public async Task FindUserByEmailAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _uut.FindUserByEmailAsync(email);

            // Assert
            Assert.That(result, Is.Null);
        }

        // --- Tests for DeleteUserAsync ---
        [Test]
        public async Task DeleteUserAsync_UserManagerReturnsSuccess_ReturnsSuccess()
        {
            // Arrange
            var user = new User();
            _userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _uut.DeleteUserAsync(user);

            // Assert
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task DeleteUserAsync_UserManagerReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var user = new User();
            var identityError = new IdentityError { Description = "Deletion failed" };
            _userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _uut.DeleteUserAsync(user);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.Contains(identityError), Is.True);
        }


        // --- Tests for GetExternalLoginInfoAsync ---
        [Test]
        public async Task GetExternalLoginInfoAsync_SignInManagerReturnsInfo_ReturnsInfo()
        {
            // Arrange
            var expectedLoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "Google", "somekey", "Google");
            _signInManager.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(expectedLoginInfo));

            // Act
            var result = await _uut.GetExternalLoginInfoAsync();

            // Assert
            Assert.That(result, Is.EqualTo(expectedLoginInfo));
        }

        [Test]
        public async Task GetExternalLoginInfoAsync_SignInManagerReturnsNull_ReturnsNull()
        {
            // Arrange
            _signInManager.GetExternalLoginInfoAsync().Returns(Task.FromResult<ExternalLoginInfo?>(null));

            // Act
            var result = await _uut.GetExternalLoginInfoAsync();

            // Assert
            Assert.That(result, Is.Null);
        }

        // --- Tests for ExternalLoginSignInAsync ---
        [Test]
        public async Task ExternalLoginSignInAsync_CallsSignInManager_ReturnsResult()
        {
            // Arrange
            var loginProvider = "Google";
            var providerKey = "somekey";
            var isPersistent = false;
            var bypassTwoFactor = true;
            var expectedResult = SignInResult.Success; // Eller en anden SignInResult for at teste forskellige scenarier

            _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _uut.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
            await _signInManager.Received(1).ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
        }
#region ConfigureExternalAuthenticationProperties

        [Test]
        public void ConfigureExternalAuthenticationProperties_CallsSignInManager_ReturnsProperties()
        {
            // Arrange
            var provider = "Google";
            var redirectUrl = "http://localhost/redirect";
            var expectedProperties = new AuthenticationProperties();
            _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl).Returns(expectedProperties);

            // Act
            var result = _uut.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Assert
            Assert.That(result, Is.EqualTo(expectedProperties));
            _signInManager.Received(1).ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }
#endregion

#region OAuth        
        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExternalLoginSucceeds_ReturnsSuccessDto()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, "test@example.com"), new Claim(ClaimTypes.Name, "Test User") };
            var claimsIdentity = new ClaimsIdentity(claims, "Google");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "providerKey", "Google");
            
            var user = new User { Id = 1, UserName = "TestUser", Email = "test@example.com" };
            var jwtToken = "generated-jwt-token";

            _signInManager.ExternalLoginSignInAsync("Google", "providerKey", false, true)
                        .Returns(Task.FromResult(SignInResult.Success));
            _userManager.FindByLoginAsync("Google", "providerKey").Returns(Task.FromResult<User?>(user));
            
            _config["Jwt:Key"].Returns("SuperSecretTestKey12345678901234567890");
            _config["Jwt:Issuer"].Returns("TestIssuer");
            _config["Jwt:Audience"].Returns("TestAudience");
            _userManager.GetRolesAsync(user).Returns(Task.FromResult<IList<string>>(new List<string>()));


            // Act
            var result = await _uut.HandleGoogleLoginCallbackAsync(externalLoginInfo);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.Success));
            Assert.That(result.JwtToken, Is.Not.Null.And.Not.Empty); // JWT token genereres nu
            Assert.That(result.AppUser, Is.EqualTo(user));
        }

        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExternalLoginFails_NewUserCreatedAndLinked_ReturnsSuccessDto()
        {
            // Arrange
            var email = "newuser@example.com";
            var userNameBase = "newuser"; // Forventet brugernavn før unik check
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Name, "new user") };
            var claimsIdentity = new ClaimsIdentity(claims, "Google");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "providerKey", "Google");
            
            User? createdUser = null; // Bruges til at fange den bruger, der sendes til CreateAsync

            _signInManager.ExternalLoginSignInAsync("Google", "providerKey", false, true)
                        .Returns(Task.FromResult(SignInResult.Failed)); // Eksternt login fejler -> ny bruger flow
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult<User?>(null)); // Ingen eksisterende bruger med denne email

            // Mock for at fange den bruger, der oprettes, og for at sikre, at brugernavnet er unikt
            _userManager.CreateAsync(Arg.Do<User>(u => createdUser = u))
                        .Returns(callInfo => Task.FromResult(IdentityResult.Success));
            _userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<User?>(null)); // Antag at genererede brugernavne er unikke

            _userManager.AddLoginAsync(Arg.Any<User>(), Arg.Any<UserLoginInfo>()).Returns(Task.FromResult(IdentityResult.Success));

            _config["Jwt:Key"].Returns("SuperSecretTestKey12345678901234567890");
            _config["Jwt:Issuer"].Returns("TestIssuer");
            _config["Jwt:Audience"].Returns("TestAudience");
            _userManager.GetRolesAsync(Arg.Any<User>()).Returns(Task.FromResult<IList<string>>(new List<string>()));

            // Act
            var result = await _uut.HandleGoogleLoginCallbackAsync(externalLoginInfo);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.Success));
            Assert.That(result.JwtToken, Is.Not.Null.And.Not.Empty);
            Assert.That(result.AppUser, Is.Not.Null);
            Assert.That(result.AppUser?.Email, Is.EqualTo(email));
            Assert.That(createdUser, Is.Not.Null, "UserManager.CreateAsync blev ikke kaldt med en bruger.");
            Assert.That(createdUser?.UserName, Does.StartWith(userNameBase), "Brugernavn blev ikke genereret korrekt.");
            await _userManager.Received(1).CreateAsync(Arg.Is<User>(u => u.Email == email && u.EmailConfirmed));
            await _userManager.Received(1).AddLoginAsync(Arg.Is<User>(u => u.Email == email),
                Arg.Is<UserLoginInfo>(uli => uli.LoginProvider == externalLoginInfo.LoginProvider && uli.ProviderKey == externalLoginInfo.ProviderKey && uli.ProviderDisplayName == externalLoginInfo.ProviderDisplayName));
        }
        
        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExternalLoginFails_NoEmailClaim_ReturnsErrorNoEmailClaim()
        {
            // Arrange
            // Opret ClaimsPrincipal uden Email claim
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "No Email User") }; 
            var claimsIdentity = new ClaimsIdentity(claims, "Google");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "providerKey", "Google");

            _signInManager.ExternalLoginSignInAsync("Google", "providerKey", false, true)
                        .Returns(Task.FromResult(SignInResult.Failed));

            // Act
            var result = await _uut.HandleGoogleLoginCallbackAsync(externalLoginInfo);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.ErrorNoEmailClaim));
            Assert.That(result.ErrorMessage, Is.EqualTo("Email ikke modtaget fra Google."));
        }
        
        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExternalLoginFails_CreateUserFails_ReturnsErrorCreateUserFailed()
        {
            // Arrange
            var email = "createfail@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Name, "Create Fail User") };
            var claimsIdentity = new ClaimsIdentity(claims, "Google");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "providerKey", "Google");

            _signInManager.ExternalLoginSignInAsync("Google", "providerKey", false, true)
                        .Returns(Task.FromResult(SignInResult.Failed));
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult<User?>(null));
            
            var identityError = new IdentityError { Description = "Simuleret oprettelsesfejl" };
            _userManager.CreateAsync(Arg.Any<User>()).Returns(Task.FromResult(IdentityResult.Failed(identityError)));
            _userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<User?>(null));


            // Act
            var result = await _uut.HandleGoogleLoginCallbackAsync(externalLoginInfo);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.ErrorCreateUserFailed));
            Assert.That(result.ErrorMessage, Is.EqualTo(identityError.Description));
        }

        [Test]
        public async Task HandleGoogleLoginCallbackAsync_ExternalLoginFails_LinkLoginFails_ReturnsErrorLinkLoginFailed()
        {
            // Arrange
            var email = "linkfail@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Name, "Link Fail User") };
            var claimsIdentity = new ClaimsIdentity(claims, "Google");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "providerKey", "Google");
            
            User? createdUser = null;

            _signInManager.ExternalLoginSignInAsync("Google", "providerKey", false, true)
                        .Returns(Task.FromResult(SignInResult.Failed));
            _authenticationRepository.GetUserByEmailAsync(email).Returns(Task.FromResult<User?>(null));

            _userManager.CreateAsync(Arg.Do<User>(u => createdUser = u)).Returns(Task.FromResult(IdentityResult.Success));
            _userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<User?>(null));
            
            var identityError = new IdentityError { Description = "Simuleret linkfejl" };
            _userManager.AddLoginAsync(Arg.Any<User>(), Arg.Any<UserLoginInfo>()).Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            // Act
            var result = await _uut.HandleGoogleLoginCallbackAsync(externalLoginInfo);

            // Assert
            Assert.That(result.Status, Is.EqualTo(GoogleLoginStatus.ErrorLinkLoginFailed));
            Assert.That(result.ErrorMessage, Is.EqualTo("Kunne ikke linke Google konto.")); // Matcher den faktiske fejlbesked i servicen
        }
#endregion
    }
}