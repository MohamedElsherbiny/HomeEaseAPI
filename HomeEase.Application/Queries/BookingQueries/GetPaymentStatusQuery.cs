using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.BookingQueries
{
    public class GetPaymentStatusQuery : IRequest<PaymentStatusDto>
    {
        public Guid BookingId { get; set; }
    }

    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

        public GetPaymentStatusQueryHandler(
            IBookingRepository bookingRepository,
            ILogger<GetPaymentStatusQueryHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<PaymentStatusDto> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking?.Payment == null)
                throw new BusinessException("Booking or payment not found");

            return new PaymentStatusDto
            {
                BookingId = booking.Id,
                Status = booking.Payment.Status,
                Amount = booking.Payment.Amount,
                Currency = booking.Payment.Currency,
                TransactionId = booking.Payment.TransactionId,
                TapChargeId = booking.Payment.TapChargeId,
                ProcessedAt = booking.Payment.ProcessedAt,
                CreatedAt = booking.Payment.CreatedAt,
                RefundedAmount = booking.Payment.RefundedAmount,
                RefundedAt = booking.Payment.RefundedAt
            };
        }
    }
}
