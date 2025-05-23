using System.Threading.Tasks;
using System.Web;
using backend.Models;
using backend.DTO.UserAuthentication;
using backend.Services.Authentication;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using static backend.Services.Authentication.EmailService;

namespace backend.Services.Authentication
{
    public interface IEmailService
    {
        public EmailDataDto GenerateRegistrationEmailAsync(string token, User user);

        public Task SendEmailAsync(string toEmail, EmailDataDto emailContent);

        public EmailDataDto GenerateResetPasswordEmailAsync(string token, User user);
    }
}
