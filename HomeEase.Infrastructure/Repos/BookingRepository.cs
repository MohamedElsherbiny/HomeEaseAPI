using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Common;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Exceptions;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Repos;

public class BookingRepository(AppDbContext _context) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Provider)
            .Include(b => b.Service)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<int> GetBookingCountByDateAsync(DateTime date)
    {
        return await _context.Bookings
            .Where(b => b.CreatedAt.Date == date.Date)
            .CountAsync();
    }

    public async Task<(List<Booking> items, int totalCount)> GetUserBookingsAsync(
     Guid userId,
      BookingStatus? status,
     DateTime? fromDate,
     DateTime? toDate,
     int page,
     int pageSize)
    {
        var query = _context.Bookings
            .Include(b => b.Provider)
            .Include(b => b.Service)
            .Include(b => b.Payment)
            .Where(b => b.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime <= toDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }


    public async Task<(List<Booking> items, int totalCount)> GetProviderBookingsAsync(
        Guid providerId,
        BookingStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        string? search)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Include(b => b.Payment)
            .Include(x => x.Provider)
            .Where(b => b.ProviderId == providerId);

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(b =>
                b.User.FirstName.ToLower().Contains(search) ||
                b.User.LastName.ToLower().Contains(search) ||
                b.User.Email.ToLower().Contains(search) ||
                b.User.PhoneNumber.ToLower().Contains(search) ||
                b.CustomerAddress.ToLower().Contains(search) ||
                b.Service.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.AppointmentDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }


    public async Task<BookingStatisticsDto> GetProviderBookingStatisticsAsync(Guid providerId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Bookings
            .Include(b => b.Service)
            .ThenInclude(s => s.BasePlatformService)
            .Include(b => b.Payment)
            .Where(b => b.ProviderId == providerId);

        if (fromDate.HasValue)
            query = query.Where(b => b.AppointmentDateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.AppointmentDateTime <= toDate.Value);

        var bookings = await query.ToListAsync();

        var statistics = new BookingStatisticsDto
        {
            TotalBookings = bookings.Count,
            CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),

            TotalRevenue = bookings
                .Where(b => b.Status == BookingStatus.Completed && b.Payment != null && b.Payment.Status == "Completed")
                .Sum(b => b.Payment.Amount),

            BookingsByBasePlatformService = bookings
                .GroupBy(b => b.Service.BasePlatformService.Name)
                .ToDictionary(g => g.Key, g => g.Count()),

            BookingsByStatusAndMonth = bookings
                .GroupBy(b => b.Status.ToString()) 
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(b => new { b.AppointmentDateTime.Year, b.AppointmentDateTime.Month })
                          .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                          .ToDictionary(
                              sg => $"{sg.Key.Year}-{sg.Key.Month:D2}",
                              sg => sg.Count()
                          )
                )

        };

        return statistics;
    }


    public async Task<bool> CheckProviderAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes, Guid? excludeBookingId = null)
    {
        // First check if the provider has this time slot available in their general availability
        var dayOfWeek = appointmentTime.DayOfWeek;
        var timeOfDay = appointmentTime.TimeOfDay;
        var endTime = timeOfDay.Add(TimeSpan.FromMinutes(durationMinutes));

        // Check if there's a recurring availability slot for this day and time
        var hasAvailabilitySlot = await _context.AvailabilitySlots
            .AnyAsync(a => a.ProviderId == providerId &&
                           (a.IsRecurring && a.DayOfWeek == dayOfWeek ||
                            !a.IsRecurring && a.SpecificDate.HasValue && a.SpecificDate.Value.Date == appointmentTime.Date) &&
                           a.StartTime <= timeOfDay && a.EndTime >= endTime);

        if (!hasAvailabilitySlot)
        {
            return false;
        }

        // Now check if there are any conflicting bookings
        var appointmentEndTime = appointmentTime.AddMinutes(durationMinutes);

        var conflictingBookingsQuery = _context.Bookings
            .Where(b => b.ProviderId == providerId &&
                       b.Status != BookingStatus.Cancelled &&
                       b.AppointmentDateTime < appointmentEndTime &&
                       b.AppointmentDateTime.AddMinutes(b.DurationMinutes) > appointmentTime);

        // Exclude the booking being updated if specified
        if (excludeBookingId.HasValue)
        {
            conflictingBookingsQuery = conflictingBookingsQuery.Where(b => b.Id != excludeBookingId.Value);
        }

        var hasConflictingBooking = await conflictingBookingsQuery.AnyAsync();

        return !hasConflictingBooking;
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var booking = await GetByIdAsync(id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
        }
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public async Task<List<PaymentInfo>> GetPendingPaymentsAsync(TimeSpan olderThan)
    {
        var thresholdTime = DateTime.UtcNow - olderThan;
        return await _context.Bookings
            .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Pending.ToString() && b.Payment.CreatedAt < thresholdTime)
            .Select(b => b.Payment)
            .ToListAsync();
    }

    public async Task UpdatePaymentAsync(PaymentInfo payment)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

        if (booking == null || booking.Payment == null)
        {
            throw new BusinessException("Booking or payment not found");
        }

        booking.Payment.Status = payment.Status;
        booking.Payment.ProcessedAt = payment.ProcessedAt;
        booking.Payment.TapChargeId = payment.TapChargeId;
        booking.Payment.TransactionId = payment.TransactionId;
        booking.Payment.ErrorCode = payment.ErrorCode;
        booking.Payment.ErrorMessage = payment.ErrorMessage;

        _context.Update(booking);
        await _context.SaveChangesAsync();
    }
}