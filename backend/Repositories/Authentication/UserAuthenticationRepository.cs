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
    
    public UserAuthenticationRepository(DataContext context, ILogger<UserAuthenticationRepository> logger)
    {
        _context = context;
    }


    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
    }

    public async Task<User?> GetUserByNameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == username.ToUpperInvariant());
    }
}