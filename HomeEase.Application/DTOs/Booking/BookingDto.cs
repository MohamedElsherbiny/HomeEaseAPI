using HomeEase.Domain.Enums;

namespace HomeEase.Application.DTOs.Booking;

public class BookingDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderBusinessName { get; set; }
    public string ProviderImageUrl { get; set; }
    public string ProviderLocationString { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string FormattedAppointmentDateTime => AppointmentDateTime.ToString("dd MMMM yyyy - hh:mm tt", new System.Globalization.CultureInfo("ar-SA"));
    public BookingStatus Status { get; set; }
    public string TranslatedStatus { get; set; }
    public string SessionLocationType { get; set; }
    public string Notes { get; set; }
    public AddressDto Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string CancellationReason { get; set; }
    public PaymentInfoDto Payment { get; set; }

}