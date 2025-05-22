using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Massage.Application.Interfaces.Services;
using Massage.Infrastructure.Data;

namespace Massage.Infrastructure.Services
{
    public class EmailService(AppDbContext _context, IConfiguration _configuration) : IEmailService
    {
        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            try
            {
                // Create a mail message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:Sender"], _configuration["EmailSettings:SenderName"]),
                    Subject = "Password Reset Request",
                    Body = $"<p>Please use the following token to reset your password:</p><p><strong>{token}</strong></p>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                // Create a SMTP client for SendinBlue
                var smtpClient = new SmtpClient("smtp-relay.sendinblue.com")
                {
                    Port = 587, // Standard SMTP port
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Sender"],
                        _configuration["EmailSettings:ApiKey"])
                };

                // Send the email
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log and rethrow the exception
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}