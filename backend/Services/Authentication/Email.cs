using System.Threading.Tasks;
using System.Web;
using backend.Models;
using backend.DTO.UserAuthentication;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace backend.Services.Authentication
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _config = config;
        }

        public EmailDataDto GenerateRegistrationEmailAsync(string token, User user)
        {
            var verificationLink = $"http://localhost:5173/verify?userId={user.Id}&token={token}";
            var subject = "Bekræft din e-mailadresse";
            var message =
                $@"<p>Tak fordi du oprettede en konto.</p>
                                    <p>Klik venligst på linket nedenfor for at bekræfte din e-mailadresse:</p>
                                    <p><a href='{verificationLink}'>Bekræft min e-mail</a></p>";

            return new EmailDataDto
            {
                ToEmail = user.Email!, 
                Subject = subject,
                HtmlMessage = message,
            };
        }

        public EmailDataDto GenerateResetPasswordEmailAsync(string token, User user)
        {
            var resetLink = $"http://localhost:5173/reset-password?userId={user.Id}&token={token}";

            var subject = "Nulstil din adgangskode";
            var message =
                $@"
                <p>Du anmodede om at nulstille din adgangskode.</p>
                <p>Klik venligst på linket nedenfor for at nulstille din adgangskode:</p>
                <p><a href='{resetLink}'>Nulstil adgangskode</a></p>";

            return new EmailDataDto
            {
                ToEmail = user.Email!,
                Subject = subject,
                HtmlMessage = message,
            };
        }

        public async Task SendEmailAsync(string toEmail, EmailDataDto emailContent)
        {
            var emailSettings = _config.GetSection("Email");

            var host = emailSettings["SmtpServer"];
            var portString = emailSettings["SmtpPort"];
            if (string.IsNullOrEmpty(portString))
            {
                throw new ArgumentNullException("SmtpPort", "SMTP port is not configured.");
            }
            var port = int.Parse(portString);
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail)); 
            message.Subject = emailContent.Subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = emailContent.HtmlMessage;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
                try
                {
                    await client.ConnectAsync(
                        host,
                        port,
                        MailKit.Security.SecureSocketOptions.StartTls
                    );  
                    await client.AuthenticateAsync(username, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending email. Error: {ex.Message}");
                    throw;
                }
        }
    }
}
