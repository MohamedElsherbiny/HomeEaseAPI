using System.Text;
using System.Text.Json;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Massage.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Massage.Infrastructure.Services
{
    public class NotificationService(
        ILogger<NotificationService> _logger,
        IUserRepository _userRepository,
        IProviderRepository _providerRepository,
        IOptions<NotificationSettings> _settings) : INotificationService
    {
        public async Task SendNotificationAsync(int userId, string title, string message)
        {
            _logger.LogInformation($"Notification to user {userId}: {title} - {message}");
            await Task.CompletedTask;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            _logger.LogInformation($"Email to {email}: {subject}");
            await Task.CompletedTask;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            _logger.LogInformation($"SMS to {phoneNumber}: {message}");
            await Task.CompletedTask;
        }

        public async Task SendBookingConfirmationAsync(Booking booking)
        {
            var user = await _userRepository.GetUserByIdAsync(booking.UserId);
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId);

            var emailModel = new EmailModel
            {
                To = user.Email,
                Subject = $"Your booking with {provider.BusinessName} has been confirmed",
                Body = $"Hello {user.FirstName},\n\n" +
                       $"Your booking for {booking.AppointmentDateTime:dddd, MMMM d, yyyy 'at' h:mm tt} has been confirmed.\n\n" +
                       $"Booking details:\n" +
                       $"Provider: {provider.BusinessName}\n" +
                       $"Service: {booking.Service.Name}\n" +
                       $"Duration: {booking.DurationMinutes} minutes\n" +
                       $"Price: ${booking.ServicePrice}\n\n" +
                       $"Thank you for using our service!"
            };

            await SendEmailModelAsync(emailModel);
        }

        public async Task SendBookingRejectionAsync(Booking booking)
        {
            var user = await _userRepository.GetUserByIdAsync(booking.UserId);
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId);

            var emailModel = new EmailModel
            {
                To = user.Email,
                Subject = $"Your booking with {provider.BusinessName} has been rejected",
                Body = $"Hello {user.FirstName},\n\n" +
                       $"Unfortunately, your booking for {booking.AppointmentDateTime:dddd, MMMM d, yyyy 'at' h:mm tt} has been rejected.\n\n" +
                       $"Reason: {booking.CancellationReason}\n\n" +
                       $"Please try booking at a different time or with another provider.\n\n" +
                       $"We apologize for any inconvenience."
            };

            await SendEmailModelAsync(emailModel);
        }

        public async Task SendProviderCancellationAsync(Booking booking)
        {
            var user = await _userRepository.GetUserByIdAsync(booking.UserId);
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId);

            var emailModel = new EmailModel
            {
                To = user.Email,
                Subject = $"Your booking with {provider.BusinessName} has been cancelled",
                Body = $"Hello {user.FirstName},\n\n" +
                       $"Unfortunately, your booking for {booking.AppointmentDateTime:dddd, MMMM d, yyyy 'at' h:mm tt} has been cancelled.\n\n" +
                       $"Reason: {booking.CancellationReason}\n\n" +
                       $"We apologize for the inconvenience."
            };

            await SendEmailModelAsync(emailModel);
        }

        public async Task SendUserCancellationAsync(Booking booking)
        {
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId);

            var emailModel = new EmailModel
            {
                To = provider.Email,
                Subject = $"A booking has been cancelled",
                Body = $"Hello,\n\n" +
                       $"A booking for {booking.AppointmentDateTime:dddd, MMMM d, yyyy 'at' h:mm tt} has been cancelled by the customer.\n\n" +
                       $"Service: {booking.Service.Name}\n" +
                       $"Duration: {booking.DurationMinutes} minutes\n" +
                       $"Reason: {booking.CancellationReason}\n\n" +
                       $"This time slot is now available for other bookings."
            };

            await SendEmailModelAsync(emailModel);
        }

        private async Task SendEmailModelAsync(EmailModel emailModel)
        {
            try
            {
                _logger.LogInformation($"Email sent to {emailModel.To}. Subject: {emailModel.Subject}");

                // Optional: call an external API (uncomment to use)
                // await CallEmailServiceApi(emailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {emailModel.To}");
            }
        }

        private async Task CallEmailServiceApi(EmailModel emailModel)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.Value.ApiKey}");

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    to = emailModel.To,
                    subject = emailModel.Subject,
                    text = emailModel.Body,
                    from = _settings.Value.FromEmail
                }),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(_settings.Value.ApiEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Email service returned {response.StatusCode}");
            }
        }
    }
}
