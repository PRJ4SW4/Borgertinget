using System.Threading.Tasks;
using System.Web;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using backend.Models;

namespace backend.Services.Authentication
{
    public interface IEmailService
    {
        public Task<EmailService> GenerateRegistrationEmailAsync(string token, User user);

        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage);

    }
}



