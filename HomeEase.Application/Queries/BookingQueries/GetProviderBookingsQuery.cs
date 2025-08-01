using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using MediatR;
namespace HomeEase.Application.Queries.BookingQueries;

public class GetProviderBookingsQuery : IRequest<PaginatedList<BookingDto>>
{
    public Guid? ProviderId { get; set; }
    public BookingStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? search { get; set; }
}

public class GetProviderBookingsQueryHandler(
    IBookingRepository _bookingRepository,
    IMapper _mapper) : IRequestHandler<GetProviderBookingsQuery, PaginatedList<BookingDto>>
{
    public async Task<PaginatedList<BookingDto>> Handle(GetProviderBookingsQuery request, CancellationToken cancellationToken)
    {

        var bookings = await _bookingRepository.GetProviderBookingsAsync(
            request.ProviderId!.Value,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize,
            request.search);

        return new PaginatedList<BookingDto>(_mapper.Map<List<BookingDto>>(bookings.items), bookings.totalCount, request.PageNumber, request.PageSize);
    }
}
