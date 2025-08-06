using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.Booking;

public class BookingConfirmationDto
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public bool IsConfirmed { get; set; }

}
