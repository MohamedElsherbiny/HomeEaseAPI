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
    // Query for platform statistics
    public class GetPlatformStatsQuery : IRequest<PlatformStatsDto>
    {
    }

    public class GetPlatformStatsQueryHandler : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
    {
        private readonly IAppDbContext _dbContext;

        public GetPlatformStatsQueryHandler(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
        {
            var totalUsers = await _dbContext.Users.CountAsync(cancellationToken);
            var activeUsers = await _dbContext.Users.CountAsync(u => u.IsActive, cancellationToken);
            var totalProviders = await _dbContext.Providers.CountAsync(cancellationToken);
            var verifiedProviders = await _dbContext.Providers.CountAsync(p => p.Status == ProviderStatus.Approved, cancellationToken);
            var totalServices = await _dbContext.Services.CountAsync(cancellationToken);
            var totalBookings = await _dbContext.Bookings.CountAsync(cancellationToken);

            // First, get the completed booking IDs
            var completedBookingIds = await _dbContext.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            // Get payment information for those bookings
            var paymentsForCompletedBookings = await _dbContext.PaymentInfos
                .Where(p => completedBookingIds.Contains(p.BookingId))
                .ToListAsync(cancellationToken);

            // Calculate total revenue
            var totalRevenue = paymentsForCompletedBookings.Sum(p => p.Amount);

            // Calculate average booking value
            var averageBookingValue = completedBookingIds.Count > 0 && paymentsForCompletedBookings.Any()
                ? paymentsForCompletedBookings.Average(p => p.Amount)
                : 0;

            double averageRating = 0;

            var ratings = await _dbContext.Reviews
                .Where(r => r.Rating.HasValue)
                .Select(r => r.Rating.Value)
                .ToListAsync(cancellationToken);

            if (ratings.Any())
            {
                averageRating = ratings.Average(r => (double)r);
            }


            return new PlatformStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalProviders = totalProviders,
                VerifiedProviders = verifiedProviders,
                TotalServices = totalServices,
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                AverageBookingValue = averageBookingValue,
                AverageRating = averageRating
            };
        }
    }
}
