using HomeEase.Domain.Entities;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message);
        Task SendEmailAsync(string email, string subject, string body);
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendBookingConfirmationAsync(Booking booking);
        Task SendBookingRejectionAsync(Booking booking);
        Task SendProviderCancellationAsync(Booking booking);
        Task SendUserCancellationAsync(Booking booking);
    }
}
