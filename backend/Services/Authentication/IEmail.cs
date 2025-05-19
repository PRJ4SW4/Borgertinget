using System.Threading.Tasks;
using System.Web;
using backend.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace backend.Services.Authentication
{
    public interface IEmailService
    {
        public Task<EmailService> GenerateRegistrationEmailAsync(string token, User user);

        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
