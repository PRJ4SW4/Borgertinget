using backend.Models;
using backend.DTO.UserAuthentication;

namespace backend.Services.Authentication
{
    public interface IEmailService
    {
        public EmailDataDto GenerateRegistrationEmailAsync(string token, User user);

        public Task SendEmailAsync(string toEmail, EmailDataDto emailContent);

        public EmailDataDto GenerateResetPasswordEmailAsync(string token, User user);
    }
}
