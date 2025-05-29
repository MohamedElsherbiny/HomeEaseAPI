using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace HomeEase.Application.DTOs
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderBusinessName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public AddressDto Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
        public PaymentInfoDto Payment { get; set; }
    }

    public class CreateBookingRequestDto
    {
        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        public string Notes { get; set; }

        //public AddressDto Location { get; set; }

        //public PaymentInfoDto PaymentInfo { get; set; }
    }

    public class UpdateBookingRequestDto
    {
        public DateTime? AppointmentDateTime { get; set; }
        public string Notes { get; set; }
        public AddressDto Location { get; set; }
    }

    public class BookingConfirmationDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public bool IsConfirmed { get; set; }

        public string Notes { get; set; }
    }

    public class BookingCancellationDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public string CancellationReason { get; set; }
    }

    public class BookingStatisticsDto
    {
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> BookingsByService { get; set; }
        public Dictionary<string, int> BookingsByMonth { get; set; }
    }
}
