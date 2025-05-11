using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendVerificationEmail(string toEmail, string verificationLink)
        {
            var fromName = _config["Email:FromName"];
            var fromEmail = _config["Email:From"];
            var smtpServer = _config["Email:SmtpServer"];
            var smtpPortString = (_config["Email:SmtpPort"]);
            var appPassword = _config["Email:AppPassword"];

            if (
                string.IsNullOrEmpty(fromName)
                || string.IsNullOrEmpty(fromEmail)
                || string.IsNullOrEmpty(smtpServer)
                || string.IsNullOrEmpty(smtpPortString)
                || string.IsNullOrEmpty(appPassword)
            )
            {
                Console.WriteLine(
                    "Advarsel: En eller flere påkrævede email-konfigurationer mangler."
                );
                // Consider throwing an exception or logging more details depending on your error handling strategy
                return;
            }

            if (!int.TryParse(smtpPortString, out int smtpPort))
            {
                Console.WriteLine($"Advarsel: Ugyldig SMTP-port konfigureret: '{smtpPortString}'.");
                // Consider throwing an exception or using a default port
                return;
            }
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Bekræft din email";

            message.Body = new TextPart("plain")
            {
                Text = $"Klik på linket for at bekræfte din konto: {verificationLink}",
            };

            using var client = new SmtpClient();
            try
            {
                client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(fromEmail, appPassword);
                client.Send(message);
                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved afsendelse af email: {ex.Message}");
                // Consider logging the full exception for debugging
            }
        }
    }
}
