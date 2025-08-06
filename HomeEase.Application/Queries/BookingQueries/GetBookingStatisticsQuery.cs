using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.Interfaces.Repos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Queries.BookingQueries;

public class GetBookingStatisticsQuery : IRequest<BookingStatisticsDto>
{
    public Guid ProviderId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetBookingStatisticsQueryHandler(
    IBookingRepository _bookingRepository,
    ILogger<GetBookingStatisticsQueryHandler> _logger) : IRequestHandler<GetBookingStatisticsQuery, BookingStatisticsDto>
{
    public async Task<BookingStatisticsDto> Handle(GetBookingStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetProviderBookingStatisticsAsync(
            request.ProviderId,
            request.FromDate,
            request.ToDate);
    }
}