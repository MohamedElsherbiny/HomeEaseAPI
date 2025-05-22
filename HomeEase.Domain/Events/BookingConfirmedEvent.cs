using HomeEase.Domain.Entities;

namespace HomeEase.Domain.Events;

public class BookingConfirmedEvent(Booking booking)
{
    public Booking Booking { get; } = booking;
}
