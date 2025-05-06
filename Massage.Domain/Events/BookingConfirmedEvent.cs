using System;
using Massage.Domain.Entities;

namespace Massage.Domain.Events
{
    public class BookingConfirmedEvent
    {
        public Booking Booking { get; }

        public BookingConfirmedEvent(Booking booking)
        {
            Booking = booking;
        }
    }
}
