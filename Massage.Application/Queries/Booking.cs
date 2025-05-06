using System;
using System.Collections.Generic;
using MediatR;
using Massage.Application.DTOs;
using AutoMapper;
using Massage.Application.Queries;
using Microsoft.Extensions.Logging;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Repos;

namespace Massage.Application.Queries
{
    public class GetBookingByIdQuery : IRequest<BookingDto>
    {
        public Guid BookingId { get; set; }
    }

    public class GetUserBookingsQuery : IRequest<List<BookingDto>>
    {
        public Guid UserId { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetProviderBookingsQuery : IRequest<List<BookingDto>>
    {
        public Guid ProviderId { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetBookingStatisticsQuery : IRequest<BookingStatisticsDto>
    {
        public Guid ProviderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}


// QUERY HANDLERS

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

    public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, List<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserBookingsQueryHandler> _logger;

        public GetUserBookingsQueryHandler(
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<GetUserBookingsQueryHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for user {request.UserId}");
                throw;
            }
        }
    }

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
