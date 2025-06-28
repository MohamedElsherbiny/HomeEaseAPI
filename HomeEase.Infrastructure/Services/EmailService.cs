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

        public async Task SendPasswordResetEmailAsync(string email, string otpCode, string lang = "ar")
        {
            string subject;
            string title;
            string body;

            if (lang == "en")
            {
                subject = "Your Password Reset Code";
                title = "Password Reset Code";
                body = $"""
                <p>Your password reset code is:</p>
                <h2>{otpCode}</h2>
                <p>This code will expire in 10 minutes.</p>
                """;
            }
            else // default to Arabic
            {
                subject = "رمز إعادة تعيين كلمة المرور";
                title = "رمز إعادة تعيين كلمة المرور";
                body = $"""
                <p>رمز إعادة تعيين كلمة المرور الخاص بك هو:</p>
                <h2>{otpCode}</h2>
                <p>سينتهي صلاحية هذا الرمز خلال 10 دقائق.</p>
                """;
            }

            var emailMessage = new EmailMassage
            {
                MailTo = [email],
                Title = title,
                Subject = subject,
                Body = body
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