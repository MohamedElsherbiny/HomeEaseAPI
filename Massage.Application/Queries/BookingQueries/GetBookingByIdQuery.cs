using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Queries.BookingQueries;
using MediatR;
using Microsoft.Extensions.Logging;


namespace Massage.Application.Queries.BookingQueries
{
    public class GetBookingByIdQuery : IRequest<BookingDto>
    {
        public Guid BookingId { get; set; }
    }
}


// QUERY HANDLER

namespace Massage.Application.Handlers.QueryHandlers
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBookingByIdQueryHandler> _logger;

        public GetBookingByIdQueryHandler(
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<GetBookingByIdQueryHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdWithDetailsAsync(request.BookingId);
                if (booking == null)
                    throw new EntityNotFoundException($"Booking with ID {request.BookingId} not found");

                return _mapper.Map<BookingDto>(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving booking {request.BookingId}");
                throw;
            }
        }
    }
}