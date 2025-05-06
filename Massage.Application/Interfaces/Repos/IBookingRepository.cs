using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Massage.Application.DTOs;
using Massage.Domain.Entities;

namespace Massage.Application.Interfaces.Repos
{
    public interface IBookingRepository
    {
        Task<Booking> GetByIdAsync(Guid id);
        Task<Booking> GetByIdWithDetailsAsync(Guid id);
        Task<List<Booking>> GetUserBookingsAsync(Guid userId, string status, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        Task<List<Booking>> GetProviderBookingsAsync(Guid providerId, string status, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        Task<BookingStatisticsDto> GetProviderBookingStatisticsAsync(Guid providerId, DateTime? fromDate, DateTime? toDate);
        Task<bool> CheckProviderAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes, Guid? excludeBookingId = null);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
