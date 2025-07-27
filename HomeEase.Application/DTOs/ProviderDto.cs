using HomeEase.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs
{
    public class ProviderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAvailableAtCenter => Services.Any(x => x.IsAvailableAtCenter);
        public bool IsAvailableAtHome => Services.Any(x => x.IsAvailableAtHome);
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public AddressDto Address { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public decimal? StartingPrice { get; set; }
        public decimal? StartingHomePrice { get; set; }
        public List<ProviderImageDto> Gallery { get; set; }
        public ProviderImageDto? Logo { get; set; }
        public ProviderImageDto? Cover { get; set; }
        public UserDto User { get; set; }

        // Navigation properties
        public ProviderScheduleDto Schedule { get; set; }
        public List<ServiceDto> Services { get; set; }

    }

    public class ProviderSearchResultDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; }
        public string ProfileImageUrl { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public AddressDto Address { get; set; }
        public decimal? Distance { get; set; }
        public List<ProviderImageDto> Gallery { get; set; }
        public ProviderImageDto? Logo { get; set; }
        public ProviderImageDto? Cover { get; set; }
    }

    public class ProviderScheduleDto
    {
        public List<WorkingHoursDto> RegularHours { get; set; }
        public List<SpecialDateDto> SpecialDates { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; }
    }

    public class WorkingHoursDto
    {
        public Guid? Id { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOpen { get; set; }
    }

    public class SpecialDateDto
    {
        public Guid? Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsClosed { get; set; }
        public string Note { get; set; }
    }

    public class TimeSlotDto
    {
        public Guid? Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class UpdateProviderDto
    {
        public string? BusinessName { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        public string? BusinessAddress { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public AddressDto? Address { get; set; }
    }
}
