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
                .Where(b => b.CreatedAt >= startDate && b.Status == BookingStatus.Completed && b.Payment != null)
                .Include(b => b.Payment)
                .AsEnumerable()
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month}",
                    g => g.Sum(b => b.Payment.Amount)
                );

            return Task.FromResult(result);
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

    // Query for booking reports
    public class GetBookingReportsQuery : IRequest<IEnumerable<AdminBookingReportDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public BookingStatus? Status { get; set; }
    }

    public class GetBookingReportsQueryHandler : IRequestHandler<GetBookingReportsQuery, IEnumerable<AdminBookingReportDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetBookingReportsQueryHandler(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AdminBookingReportDto>> Handle(GetBookingReportsQuery request, CancellationToken cancellationToken)
        {
            var query = from booking in _dbContext.Bookings
                        join user in _dbContext.Users on booking.UserId equals user.Id
                        join provider in _dbContext.Providers on booking.ProviderId equals provider.Id
                        join providerUser in _dbContext.Users on provider.UserId equals providerUser.Id
                        join service in _dbContext.Services on booking.ServiceId equals service.Id
                        join payment in _dbContext.PaymentInfos on booking.Id equals payment.BookingId into payments
                        from payment in payments.DefaultIfEmpty()
                        select new AdminBookingReportDto
                        {
                            BookingId = booking.Id,
                            UserId = booking.UserId,
                            UserName = $"{user.FirstName} {user.LastName}",
                            ProviderId = booking.ProviderId,
                            ProviderName = provider.BusinessName ?? $"{providerUser.FirstName} {providerUser.LastName}",
                            ServiceId = booking.ServiceId,
                            ServiceName = service.Name,
                            Amount = payment != null ? payment.Amount : 0,
                            BookingDate = booking.AppointmentDate,
                            Status = booking.Status.ToString(),
                            CreatedAt = booking.CreatedAt,
                            Rating = _dbContext.Reviews
                                .Where(r => r.BookingId == booking.Id)
                                .Select(r => r.Rating.HasValue ? (double?)r.Rating.Value : null)
                                .FirstOrDefault()
                        };

            // Apply filters
            if (request.StartDate.HasValue)
            {
                query = query.Where(b => b.BookingDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(b => b.BookingDate <= request.EndDate.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(b => b.Status == request.Status.Value.ToString());
            }

            return await query.ToListAsync(cancellationToken);
        }
    }

    // Query for provider reports
    public class GetProviderReportsQuery : IRequest<IEnumerable<AdminProviderReportDto>>
    {
        public ProviderStatus? Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetProviderReportsQueryHandler : IRequestHandler<GetProviderReportsQuery, IEnumerable<AdminProviderReportDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProviderReportsQueryHandler(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AdminProviderReportDto>> Handle(GetProviderReportsQuery request, CancellationToken cancellationToken)
        {
            var query = from provider in _dbContext.Providers
                        join user in _dbContext.Users on provider.UserId equals user.Id
                        select new
                        {
                            Provider = provider,
                            User = user,
                            ServicesCount = _dbContext.Services.Count(s => s.ProviderId == provider.Id),
                            BookingsCount = _dbContext.Bookings.Count(b => b.ProviderId == provider.Id),
                            TotalRevenue = _dbContext.Bookings
                                .Where(b => b.ProviderId == provider.Id && b.Status == BookingStatus.Completed)
                                .Join(_dbContext.PaymentInfos, b => b.Id, p => p.BookingId, (b, p) => p.Amount)
                                .Sum(),
                            AverageRating = _dbContext.Reviews
                                .Where(r => r.ProviderId == provider.Id && r.Rating.HasValue)
                                .Average(r => (double)r.Rating.Value),
                            LastActive = _dbContext.Reviews
                                .Where(r => r.ProviderId == provider.Id)
                                .OrderByDescending(r => r.UpdatedAt)
                                .Select(r => r.UpdatedAt)
                                .FirstOrDefault()
                        };

            // Apply filters
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Provider.Status == request.Status.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.User.IsActive == request.IsActive.Value);
            }

            var result = await query.Select(x => new AdminProviderReportDto
            {
                ProviderId = x.Provider.Id,
                UserId = x.User.Id,
                ProviderName = x.Provider.BusinessName ?? $"{x.User.FirstName} {x.User.LastName}",
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                Status = x.Provider.Status.ToString(),
                ServicesCount = x.ServicesCount,
                BookingsCount = x.BookingsCount,
                TotalRevenue = x.TotalRevenue,
                AverageRating = x.AverageRating,
                CreatedAt = x.Provider.CreatedAt,
                LastActive = x.LastActive
            }).ToListAsync(cancellationToken);

            return result;
        }
    }

    // Query for user reports
    public class GetUserReportsQuery : IRequest<IEnumerable<AdminUserReportDto>>
    {
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetUserReportsQueryHandler : IRequestHandler<GetUserReportsQuery, IEnumerable<AdminUserReportDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetUserReportsQueryHandler(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AdminUserReportDto>> Handle(GetUserReportsQuery request, CancellationToken cancellationToken)
        {
            var query = from user in _dbContext.Users
                        select new
                        {
                            User = user,
                            BookingsCount = _dbContext.Bookings.Count(b => b.UserId == user.Id),
                            TotalSpent = _dbContext.Bookings
                                .Where(b => b.UserId == user.Id && b.Status == BookingStatus.Completed)
                                .Join(_dbContext.PaymentInfos, b => b.Id, p => p.BookingId, (b, p) => p.Amount)
                                .Sum(),
                            //LastLogin = user.LastLoginDate
                        };

            // Apply filters
            if (request.Role.HasValue)
            {
                query = query.Where(x => x.User.Role == request.Role.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.User.IsActive == request.IsActive.Value);
            }

            var result = await query.Select(x => new AdminUserReportDto
            {
                UserId = x.User.Id,
                Email = x.User.Email,
                FullName = $"{x.User.FirstName} {x.User.LastName}",
                PhoneNumber = x.User.PhoneNumber,
                Role = x.User.Role.ToString(),
                IsActive = x.User.IsActive,
                BookingsCount = x.BookingsCount,
                TotalSpent = x.TotalSpent,
                CreatedAt = x.User.CreatedAt,
                //LastLogin = x.LastLogin
            }).ToListAsync(cancellationToken);

            return result;
        }
    }

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

            var averageRating = await _dbContext.Reviews
                .Where(r => r.Rating.HasValue)
                .AverageAsync(r => (double)r.Rating.Value, cancellationToken);

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
