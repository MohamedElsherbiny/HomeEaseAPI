using System;
using Massage.Domain.Entities;

namespace Massage.Domain.Events
{
    public class BookingCreatedEvent
    {
        public Booking Booking { get; }

        public BookingCreatedEvent(Booking booking)
        {
            Booking = booking;
        }
    }
}