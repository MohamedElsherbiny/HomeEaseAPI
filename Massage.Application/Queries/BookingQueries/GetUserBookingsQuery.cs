using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Repos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Queries.BookingQueries;

public class GetUserBookingsQuery : IRequest<List<BookingDto>>
{
    public Guid UserId { get; set; }
    public string Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetUserBookingsQueryHandler(
    IBookingRepository _bookingRepository,
    IMapper _mapper,
    ILogger<GetUserBookingsQueryHandler> _logger) : IRequestHandler<GetUserBookingsQuery, List<BookingDto>>
{
    public async Task<List<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {

        var bookings = await _bookingRepository.GetUserBookingsAsync(
            request.UserId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize);

        return _mapper.Map<List<BookingDto>>(bookings);
    }
}