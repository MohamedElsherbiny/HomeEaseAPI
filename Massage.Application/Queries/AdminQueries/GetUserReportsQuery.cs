using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.AdminQueries
{
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
}
