using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Repos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Queries.BookingQueries;

public class GetProviderBookingsQuery : IRequest<List<BookingDto>>
{
    public Guid ProviderId { get; set; }
    public string Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetProviderBookingsQueryHandler(
    IBookingRepository _bookingRepository,
    IMapper _mapper,
    ILogger<GetProviderBookingsQueryHandler> _logger) : IRequestHandler<GetProviderBookingsQuery, List<BookingDto>>
{
    public async Task<List<BookingDto>> Handle(GetProviderBookingsQuery request, CancellationToken cancellationToken)
    {

        var bookings = await _bookingRepository.GetProviderBookingsAsync(
            request.ProviderId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize);

        return _mapper.Map<List<BookingDto>>(bookings);
    }
}
