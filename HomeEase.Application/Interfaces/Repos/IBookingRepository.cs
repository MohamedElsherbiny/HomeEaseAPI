using HomeEase.Application.DTOs;
using HomeEase.Domain.Common;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Repos
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task<Booking> GetByIdWithDetailsAsync(Guid id);
        Task<int> GetMaxSerialNumberAsync();
        Task<(List<Booking> items, int totalCount)> GetUserBookingsAsync(Guid userId, BookingStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize);        Task<(List<Booking> items, int totalCount)> GetProviderBookingsAsync(Guid providerId, BookingStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? search);

        Task<BookingStatisticsDto> GetProviderBookingStatisticsAsync(Guid providerId, DateTime? fromDate, DateTime? toDate);
        Task<bool> CheckProviderAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes, Guid? excludeBookingId = null);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
        Task<List<PaymentInfo>> GetPendingPaymentsAsync(TimeSpan olderThan);
        Task UpdatePaymentAsync(PaymentInfo payment);
    }
}
