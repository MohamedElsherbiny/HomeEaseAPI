using AutoMapper;
using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;


namespace HomeEase.Application.Queries.BookingQueries;

public class GetBookingByIdQuery : IRequest<BookingDto>
{
    public Guid BookingId { get; set; }
}



public class GetBookingByIdQueryHandler(
    IBookingRepository _bookingRepository,
    IMapper _mapper,
    ILogger<GetBookingByIdQueryHandler> _logger) : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(request.BookingId);
        if (booking is null)
        {
            throw new BusinessException($"Booking with ID {request.BookingId} not found");
        }

        return _mapper.Map<BookingDto>(booking);
    }
}