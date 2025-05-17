namespace backend.Repositories.Authentication;

using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

// Implements repository operations for CalendarEvent entities using Entity Framework Core.
public class UserAuthenticationRepository : IUserAuthenticationRepository
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserAuthenticationRepository> _logger;
    public UserAuthenticationRepository(DataContext context, UserManager<User> userManager, ILogger<UserAuthenticationRepository> logger)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        _logger.LogInformation("Fetching user by ID: {UserId} from database.", userId);
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        _logger.LogInformation("Attempting to find user by email in repository: {Email}", email);
        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
    }

    public async Task<User?> FindUserByNameAsync(string username)
    {
        _logger.LogInformation("Attempting to find user by username in repository: {Username}", username);
        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == username.ToUpperInvariant());
    }
    public async Task<User?> FindUserByLoginAsync(string loginProvider, string providerKey)
    {
        _logger.LogInformation("Finding user by login: Provider={LoginProvider}, Key={ProviderKey}", loginProvider, providerKey);
        return await _userManager.FindByLoginAsync(loginProvider, providerKey);
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string? password = null)
    {
        _logger.LogInformation("Creating new user in repository: {UserName}", user.UserName);
        if (string.IsNullOrEmpty(password))
        {
            return await _userManager.CreateAsync(user);
        }
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo login)
    {
        _logger.LogInformation("Adding external login for user: {UserName}, Provider: {LoginProvider}", user.UserName, login.LoginProvider);
        return await _userManager.AddLoginAsync(user, login);
    }
    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        _logger.LogInformation("Updating user in repository: {UserName}", user.UserName);
        return await _userManager.UpdateAsync(user);
    }
}