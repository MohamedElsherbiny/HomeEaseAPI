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
    public class GetBookingStatisticsQuery : IRequest<BookingStatisticsDto>
    {
        public Guid ProviderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}


// QUERY HANDLER

namespace Massage.Application.Handlers.QueryHandlers
{
    public class GetBookingStatisticsQueryHandler : IRequestHandler<GetBookingStatisticsQuery, BookingStatisticsDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<GetBookingStatisticsQueryHandler> _logger;

        public GetBookingStatisticsQueryHandler(
            IBookingRepository bookingRepository,
            ILogger<GetBookingStatisticsQueryHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<BookingStatisticsDto> Handle(GetBookingStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _bookingRepository.GetProviderBookingStatisticsAsync(
                    request.ProviderId,
                    request.FromDate,
                    request.ToDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving booking statistics for provider {request.ProviderId}");
                throw;
            }
        }
    }
}
