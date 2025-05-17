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
}
