namespace backend.Repositories.Authentication;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Models.Calendar;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

// Defines a contract for repository operations related to CalendarEvent entities.
public interface IUserAuthenticationRepository
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> FindUserByEmailAsync(string email);
    Task<User?> FindUserByNameAsync(string username);
    Task<User?> FindUserByLoginAsync(string loginProvider, string providerKey);
    Task<IdentityResult> CreateUserAsync(User user, string? password = null);
    Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo login);
    Task<IdentityResult> UpdateUserAsync(User user);
    Task<IdentityResult> GiveUserRoleAsync(User user, string role);
}