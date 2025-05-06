using System;
using System.ComponentModel.DataAnnotations;

namespace Massage.Application.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderBusinessName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public AddressDto Location { get; set; }
        public bool ReminderSent { get; set; }
        public bool FollowUpSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool HasReview { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public string Status { get; set; }

        public string Notes { get; set; }
    }

    public class AppointmentReminderDto
    {
        public Guid AppointmentId { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserFullName { get; set; }
        public string ProviderBusinessName { get; set; }
        public string ServiceName { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public AddressDto Location { get; set; }
    }

    public class AppointmentCalendarDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; }
        public bool IsAllDay { get; set; }
        public string Color { get; set; }
    }

    public class AppointmentAvailabilityRequestDto
    {
        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class AppointmentAvailabilityResponseDto
    {
        public Guid ProviderId { get; set; }
        public Guid ServiceId { get; set; }
        public int ServiceDurationMinutes { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; }
    }
}