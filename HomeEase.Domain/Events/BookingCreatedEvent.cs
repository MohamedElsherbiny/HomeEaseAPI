using HomeEase.Domain.Entities;

namespace HomeEase.Domain.Events;

public class BookingCreatedEvent(Booking booking)
{
    public Booking Booking { get; } = booking;
}