using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.Booking;

public class BookingCancellationDto
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public string CancellationReason { get; set; }
}
