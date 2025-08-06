using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.Booking;

public class CreateBookingRequestDto
{
    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan AppointmentTime { get; set; }

    public string? Notes { get; set; }

    public int DurationMinutes { get; set; }
    public bool IsHomeService { get; set; }
    public string? CustomerAddress { get; set; } // For home service bookings

}
