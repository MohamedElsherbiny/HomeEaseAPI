using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Queries.BookingQueries;

public class GetBookingStatusCountsQuery : IRequest<EntityResult>
{
    public Guid ProviderId { get; set; }
}

public class GetBookingStatusCountsQueryHandler(IAppDbContext _context, ICurrentUserService currentUserService) : IRequestHandler<GetBookingStatusCountsQuery, EntityResult>
{
    public async Task<EntityResult> Handle(GetBookingStatusCountsQuery request, CancellationToken cancellationToken)
    {
        var allStatuses = Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>();
        var counts = await _context.Bookings
            .Where(x => x.ProviderId == request.ProviderId)
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);

        var result = allStatuses.Select(status =>
        {
            counts.TryGetValue(status, out var count);

            return new BookingStatusCountDto
            {
                StatusId = (int)status,
                StatusName = EnumTranslations.TranslateBookingStatus(status, currentUserService.Language),
                Count = count
            };
        }).ToList();

        return EntityResult.SuccessWithData(result);
    }
}