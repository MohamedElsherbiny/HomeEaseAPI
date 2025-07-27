using HomeEase.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace HomeEase.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? AddressId { get; set; }
    public string? CustomerAddress { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public bool IsHomeService { get; set; }
    public BookingStatus Status { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal ServicePrice { get; set; }
    public decimal ServiceHomePrice { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; }

    // Navigation properties
    public virtual User User { get; set; }
    public virtual Provider Provider { get; set; }
    public virtual Service Service { get; set; }
    public virtual Address Address { get; set; }
    public virtual Review Review { get; set; }
    public PaymentInfo Payment { get; set; }
}
