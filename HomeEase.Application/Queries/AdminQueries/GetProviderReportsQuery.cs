using AutoMapper;
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
                                .Select(r => (decimal?)r.Rating.Value)
                                .Average() ?? 0,
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
}
