using System.Threading.Tasks;
using System.Web;
using backend.Models;
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
        public EmailData GenerateRegistrationEmailAsync(string token, User user);

        public Task SendEmailAsync(string toEmail, EmailData emailContent);

        public EmailData GenerateResetPasswordEmailAsync(string token, User user);
    }
}
