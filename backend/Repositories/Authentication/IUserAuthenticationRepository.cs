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
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByNameAsync(string username);
}