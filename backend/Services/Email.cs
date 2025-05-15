using System.Threading.Tasks;
using System.Web;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
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
            message.To.Add(new MailboxAddress("", toEmail)); // Modtagernavn kan være tomt
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlMessage;
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
