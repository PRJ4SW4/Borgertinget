namespace backend.Repositories.Authentication;

using System.Threading.Tasks;
using backend.Models;

public interface IUserAuthenticationRepository
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByNameAsync(string username);
}