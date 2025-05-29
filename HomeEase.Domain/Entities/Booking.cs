using HomeEase.Domain.Enums;

namespace HomeEase.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? LocationId { get; set; }
    public string? CustomerAddress { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public bool IsHomeService { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public int DurationMinutes { get; set; }
    public decimal ServicePrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
    public DateTime AppointmentDateTime { get; set; }


    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Provider Provider { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
    public virtual Location? Location { get; set; }
    public virtual Review? Review { get; set; }
    public virtual ICollection<PaymentInfo> Payments { get; set; } = new List<PaymentInfo>();
}