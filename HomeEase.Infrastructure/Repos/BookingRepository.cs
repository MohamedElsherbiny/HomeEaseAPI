using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Repos;

public class BookingRepository(AppDbContext _context) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Provider)
            .Include(b => b.Service)
            .Include(b => b.Location)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetUserBookingsAsync(Guid userId, string status, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.Bookings
            .Include(b => b.Provider)
            .Include(b => b.Service)
            .Include(b => b.Location)
            .Include(b => b.Payment)
            .Where(b => b.UserId == userId);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<BookingStatus>(status, out var parsedStatus))
            {
                query = query.Where(b => b.Status == parsedStatus);
            }
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime <= toDate.Value);
        }

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetProviderBookingsAsync(Guid providerId, string status, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Include(b => b.Location)
            .Include(b => b.Payment)
            .Where(b => b.ProviderId == providerId);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<BookingStatus>(status, out var parsedStatus))
            {
                query = query.Where(b => b.Status == parsedStatus);
            }
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime <= toDate.Value);
        }

        return await query
            .OrderByDescending(b => b.AppointmentDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<BookingStatisticsDto> GetProviderBookingStatisticsAsync(Guid providerId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Bookings
            .Include(b => b.Service)
            .Include(b => b.Payment)
            .Where(b => b.ProviderId == providerId);

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.AppointmentDateTime <= toDate.Value);
        }

        var bookings = await query.ToListAsync();

        var statistics = new BookingStatisticsDto
        {
            TotalBookings = bookings.Count,
            CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            TotalRevenue = bookings
                .Where(b => b.Status == BookingStatus.Completed && b.Payment != null && b.Payment.Status == "Completed")
                .Sum(b => b.Payment.Amount),
            BookingsByService = bookings
                .GroupBy(b => b.Service.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            BookingsByMonth = bookings
                .GroupBy(b => new { b.AppointmentDateTime.Year, b.AppointmentDateTime.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month:D2}",
                    g => g.Count())
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
}