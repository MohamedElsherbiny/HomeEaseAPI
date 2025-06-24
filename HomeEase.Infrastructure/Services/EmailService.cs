using Azure;
using HomeEase.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Azure.Communication.Email;

namespace HomeEase.Infrastructure.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly EmailClient _emailClient = new(configuration["EmailService:ConnectionString"]);
        private readonly string _senderAddress = configuration["EmailService:SenderAddress"]!;

        public Task SendEmailAsync(EmailMassage email)
        {
            var emailMessage = new EmailMessage(
                _senderAddress,
                content: new EmailContent(email.Subject)
                {
                    Html = email.Body
                },
                recipients: new EmailRecipients(email.MailTo.Select(x => new EmailAddress(x))));

            _emailClient.Send(WaitUntil.Completed, emailMessage);

            return Task.CompletedTask;
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var emailMessage = new EmailMassage
            {
                MailTo = [email],
                Title = "Password Reset Request",
                Subject = "Password Reset Request",
                Body = $"<p>Please use the following token to reset your password:</p><p><strong>{token}</strong></p>"
            };

            await SendEmailAsync(emailMessage);
        }
    }

    public class EmailMassage
    {
        public List<string> MailTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
    }
}