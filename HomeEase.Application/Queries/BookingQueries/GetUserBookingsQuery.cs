using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using MediatR;

namespace HomeEase.Application.Queries.BookingQueries;

public class GetUserBookingsQuery : IRequest<PaginatedList<BookingDto>>
{
    public Guid? UserId { get; set; }
    public BookingStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetUserBookingsQueryHandler(IBookingRepository _bookingRepository, IMapper _mapper) : IRequestHandler<GetUserBookingsQuery, PaginatedList<BookingDto>>
{
    public async Task<PaginatedList<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bookingRepository.GetUserBookingsAsync(
            request.UserId!.Value,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize);

        return new PaginatedList<BookingDto>(_mapper.Map<List<BookingDto>>(items), totalCount, request.PageNumber, request.PageSize);
    }
}