using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Massage.Application.DTOs
{
    public class ProviderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string[] ServiceTypes { get; set; }
        public AddressDto Address { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public ProviderScheduleDto Schedule { get; set; }
        public List<ServiceDto> Services { get; set; }
    }

    public class ProviderSearchResultDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string[] ServiceTypes { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public AddressDto Address { get; set; }
        public decimal? Distance { get; set; }
    }

    public class ProviderScheduleDto
    {
        public List<WorkingHoursDto> RegularHours { get; set; }
        public List<SpecialDateDto> SpecialDates { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; }
    }

    public class WorkingHoursDto
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOpen { get; set; }
    }

    public class SpecialDateDto
    {
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsClosed { get; set; }
        public string Note { get; set; }
    }

    public class TimeSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class UpdateProviderDto
    {
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string[] ServiceTypes { get; set; }
        public AddressDto Address { get; set; }
    }

    public class ServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string ServiceType { get; set; }
    }

    public class CreateServiceDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(15, 300)]
        public int DurationMinutes { get; set; }

        //[Required]
        //public string ServiceType { get; set; }
    }

    public class UpdateServiceDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? DurationMinutes { get; set; }
        public string ServiceType { get; set; }
    }
}
