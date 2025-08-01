using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using HomeEase.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeEase.Infrastructure.Services
{
    public class NotificationService(
        ILogger<NotificationService> _logger,
        HttpClient _httpClient,
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

        public async Task SendPaymentConfirmationAsync(string email, Guid bookingId, decimal amount, string currency)
        {
            try
            {
                var emailModel = new EmailModel
                {
                    To = email,
                    Subject = "Payment Confirmation - HomeEase Massage Booking",
                    Body = $@"
                    <h2>Payment Confirmed</h2>
                    <p>Your payment has been successfully processed.</p>
                    <p><strong>Booking ID:</strong> {bookingId}</p>
                    <p><strong>Amount:</strong> {amount} {currency}</p>
                    <p><strong>Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>
                    <p>Thank you for choosing HomeEase!</p>
                "
                };

                await SendEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send payment confirmation email to {email}");
            }
        }

        public async Task SendPaymentFailureNotificationAsync(string email, Guid bookingId, string errorMessage)
        {
            try
            {
                var emailModel = new EmailModel
                {
                    To = email,
                    Subject = "Payment Failed - HomeEase Massage Booking",
                    Body = $@"
                    <h2>Payment Failed</h2>
                    <p>Unfortunately, your payment could not be processed.</p>
                    <p><strong>Booking ID:</strong> {bookingId}</p>
                    <p><strong>Error:</strong> {errorMessage}</p>
                    <p>Please try again or contact our support team.</p>
                "
                };

                await SendEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send payment failure email to {email}");
            }
        }

        public async Task SendRefundConfirmationAsync(string email, Guid bookingId, decimal refundAmount, string currency)
        {
            try
            {
                var emailModel = new EmailModel
                {
                    To = email,
                    Subject = "Refund Processed - HomeEase Massage Booking",
                    Body = $@"
                    <h2>Refund Processed</h2>
                    <p>Your refund has been successfully processed.</p>
                    <p><strong>Booking ID:</strong> {bookingId}</p>
                    <p><strong>Refund Amount:</strong> {refundAmount} {currency}</p>
                    <p><strong>Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>
                    <p>The refunded amount will appear in your account within 3-5 business days.</p>
                "
                };

                await SendEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send refund confirmation email to {email}");
            }
        }

        private async Task SendEmailAsync(EmailModel emailModel)
        {
            // Implementation depends on your email service provider
            // This is a generic HTTP client example
            var payload = new
            {
                to = emailModel.To,
                from = _settings.Value.FromEmail,
                subject = emailModel.Subject,
                html = emailModel.Body
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.Value.ApiKey);

            await _httpClient.PostAsync(_settings.Value.ApiEndpoint, content);

        }
    }
}
