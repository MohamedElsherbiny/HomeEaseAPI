using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.AdminQueries
{
    // Query for admin dashboard statistics
    public class GetDashboardStatsQuery : IRequest<AdminDashboardStatsDto>
    {
    }

    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, AdminDashboardStatsDto>
    {
        private readonly IAppDbContext _dbContext;

        public GetDashboardStatsQueryHandler(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminDashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var totalUsers = await _dbContext.Users.CountAsync(u => u.Role == UserRole.User);
            var totalProviders = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Provider);
            var pendingProviders = await _dbContext.Providers.CountAsync(p => p.Status == ProviderStatus.Pending);
            var totalBookings = await _dbContext.Bookings.CountAsync();
            var completedBookings = await _dbContext.Bookings.CountAsync(b => b.Status == BookingStatus.Completed);

            // Get revenue calculations (assuming there's a payment amount field in booking)
            var totalRevenue = await _dbContext.PaymentInfos
                .Where(p => _dbContext.Bookings
                    .Any(b => b.Id == p.BookingId && b.Status == BookingStatus.Completed))
                .SumAsync(p => p.Amount);


            var recentBookings = await _dbContext.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            return new AdminDashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalProviders = totalProviders,
                PendingProviders = pendingProviders,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                TotalRevenue = totalRevenue,
                BookingsPerMonth = await GetBookingsPerMonth(),
                RevenuePerMonth = await GetRevenuePerMonth()
            };
        }

        private Task<Dictionary<string, int>> GetBookingsPerMonth()
        {
            var startDate = DateTime.UtcNow.AddMonths(-6);

            var result = _dbContext.Bookings
                .Where(b => b.CreatedAt >= startDate)
                .AsEnumerable()
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month}",
                    g => g.Count()
                );

            return Task.FromResult(result);
        }


        private Task<Dictionary<string, decimal>> GetRevenuePerMonth()
        {
            var startDate = DateTime.UtcNow.AddMonths(-6);

            var result = _dbContext.Bookings
                .Where(b => b.CreatedAt >= startDate && b.Status == BookingStatus.Completed && b.Payments.Any())
                .Include(b => b.Payments)
                .AsEnumerable()
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month}",
                    g => g.Sum(b => b.Payments.Sum(p => p.Amount))
                );

            return Task.FromResult(result);
        }

    }
}
