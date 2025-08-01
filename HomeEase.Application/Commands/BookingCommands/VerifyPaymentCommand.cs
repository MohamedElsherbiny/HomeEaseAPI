using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.BookingCommands
{
    public class VerifyPaymentCommand : IRequest<PaymentResult>
    {
        public Guid BookingId { get; set; }
    }

    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, PaymentResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly ILogger<VerifyPaymentCommandHandler> _logger;

        public VerifyPaymentCommandHandler(
            IBookingRepository bookingRepository,
            IPaymentProcessor paymentProcessor,
            ILogger<VerifyPaymentCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _paymentProcessor = paymentProcessor;
            _logger = logger;
        }

        public async Task<PaymentResult> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking?.Payment == null)
                throw new BusinessException("Booking or payment not found");

            if (string.IsNullOrEmpty(booking.Payment.TapChargeId))
                throw new BusinessException("No Tap charge ID found for verification");

            // Get current status from Tap Gateway
            var result = await _paymentProcessor.GetPaymentStatusAsync(booking.Payment.TapChargeId);

            // Update local payment record if status changed
            if (booking.Payment.Status != result.Status.ToString())
            {
                booking.Payment.Status = result.Status.ToString();
                if (result.IsSuccessful && !booking.Payment.ProcessedAt.HasValue)
                {
                    booking.Payment.ProcessedAt = DateTime.UtcNow;
                }

                await _bookingRepository.UpdateAsync(booking);
                await _bookingRepository.SaveChangesAsync();

                _logger.LogInformation($"Payment status updated for booking {booking.Id}: {result.Status}");
            }

            return result;
        }
    }
}
