using Massage.Domain.Entities;

namespace Massage.Domain.Events;

public class BookingConfirmedEvent(Booking booking)
{
    public Booking Booking { get; } = booking;
}
