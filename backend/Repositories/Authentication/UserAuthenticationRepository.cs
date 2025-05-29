namespace backend.Repositories.Authentication;

using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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