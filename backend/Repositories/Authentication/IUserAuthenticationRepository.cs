namespace backend.Repositories.Authentication;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Models.Calendar;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public interface IUserAuthenticationRepository
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByNameAsync(string username);
}