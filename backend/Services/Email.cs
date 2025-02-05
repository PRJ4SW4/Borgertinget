using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace UserAuthentication.Services
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
            Console.WriteLine($"Sender email til {toEmail} med link: {verificationLink}");
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Bekræft din email";

            message.Body = new TextPart("plain")
            {
                Text = $"Klik på linket for at bekræfte din konto: {verificationLink}"
            };

            using var client = new SmtpClient();
            client.Connect(_config["Email:SmtpServer"], int.Parse(_config["Email:SmtpPort"]), SecureSocketOptions.StartTls);
            client.Authenticate(_config["Email:From"], _config["Email:AppPassword"]);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
