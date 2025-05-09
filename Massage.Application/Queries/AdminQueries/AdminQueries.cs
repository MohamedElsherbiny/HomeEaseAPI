using MediatR;
using Massage.Application.Interfaces.Repos;
using Massage.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Massage.Application.DTOs;
using AutoMapper;
using Massage.Application.Interfaces;

namespace Massage.Application.Queries.AdminQueries
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

        private async Task<Dictionary<string, int>> GetBookingsPerMonth()
        {
            var startDate = DateTime.UtcNow.AddMonths(-6);
            var bookings = await _dbContext.Bookings
                .Where(b => b.CreatedAt >= startDate)
                .ToListAsync();

            return bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month}",
                    g => g.Count()
                );
        }

        private async Task<Dictionary<string, decimal>> GetRevenuePerMonth()
        {
            var startDate = DateTime.UtcNow.AddMonths(-6);
            var bookings = await _dbContext.Bookings
                .Where(b => b.CreatedAt >= startDate && b.Status == BookingStatus.Completed)
                .ToListAsync();

            return await _dbContext.Bookings
                .Where(b => b.Status == BookingStatus.Completed && b.Payment != null)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionaryAsync(
                    g => $"{g.Key.Year}-{g.Key.Month}",
                    g => g.Sum(b => b.Payment.Amount)
                );

        }
    }

    // Query to get users with filters for admin
    public class GetFilteredUsersQuery : IRequest<List<UserDto>>
    {
        public string? SearchTerm { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetFilteredUsersQueryHandler : IRequestHandler<GetFilteredUsersQuery, List<UserDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetFilteredUsersQueryHandler(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.Email.Contains(request.SearchTerm) ||
                    u.FirstName.Contains(request.SearchTerm) ||
                    u.LastName.Contains(request.SearchTerm) ||
                    u.PhoneNumber.Contains(request.SearchTerm));
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.Role == request.Role.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= request.ToDate.Value);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
