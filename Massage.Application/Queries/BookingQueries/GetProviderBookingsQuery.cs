using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Queries.BookingQueries;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.BookingQueries
{
    public class GetProviderBookingsQuery : IRequest<List<BookingDto>>
    {
        public Guid ProviderId { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

// QUERY HANDLER

namespace Massage.Application.Handlers.QueryHandlers
{
    public class GetProviderBookingsQueryHandler : IRequestHandler<GetProviderBookingsQuery, List<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProviderBookingsQueryHandler> _logger;

        public GetProviderBookingsQueryHandler(
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<GetProviderBookingsQueryHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<BookingDto>> Handle(GetProviderBookingsQuery request, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for provider {request.ProviderId}");
                throw;
            }
        }
    }
}
