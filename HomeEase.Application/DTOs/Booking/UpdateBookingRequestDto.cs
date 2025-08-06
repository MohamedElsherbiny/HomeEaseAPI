namespace HomeEase.Application.DTOs.Booking;

public class UpdateBookingRequestDto
{
    public DateTime? AppointmentDateTime { get; set; }
    public string? Notes { get; set; }
    public AddressDto Address { get; set; }
}
