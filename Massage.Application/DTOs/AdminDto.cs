using System;
using System.Collections.Generic;

namespace Massage.Application.DTOs
{
    public class AdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalProviders { get; set; }
        public int PendingProviders { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> BookingsPerMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenuePerMonth { get; set; } = new Dictionary<string, decimal>();
    }

    public class PlatformStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalProviders { get; set; }
        public int VerifiedProviders { get; set; }
        public int TotalServices { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
        public double AverageRating { get; set; }
    }

    public class AdminBookingReportDto
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Amount { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public double? Rating { get; set; }
    }

    public class AdminUserReportDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public int BookingsCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AdminProviderReportDto
    {
        public Guid ProviderId { get; set; }
        public Guid UserId { get; set; }
        public string ProviderName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public int ServicesCount { get; set; }
        public int BookingsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActive { get; set; }
    }
}
