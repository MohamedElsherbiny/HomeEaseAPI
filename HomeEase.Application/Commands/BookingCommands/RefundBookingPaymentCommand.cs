using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
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
    public class RefundBookingPaymentCommand : IRequest<RefundResult>
    {
        public Guid BookingId { get; set; }
        public decimal? RefundAmount { get; set; } // null for full refund
        public string Reason { get; set; } = "requested_by_customer";
    }

    public class RefundBookingPaymentCommandHandler : IRequestHandler<RefundBookingPaymentCommand, RefundResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RefundBookingPaymentCommandHandler> _logger;

        public RefundBookingPaymentCommandHandler(
            IBookingRepository bookingRepository,
            IPaymentProcessor paymentProcessor,
            INotificationService notificationService,
            ILogger<RefundBookingPaymentCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _paymentProcessor = paymentProcessor;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<RefundResult> Handle(RefundBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
                if (booking?.Payment == null)
                    throw new BusinessException("Booking or payment not found");

                if (string.IsNullOrEmpty(booking.Payment.TapChargeId))
                    throw new BusinessException("No valid charge ID found for refund");

                if (booking.Payment.Status != "Completed")
                    throw new BusinessException("Can only refund completed payments");

                var refundAmount = request.RefundAmount ?? booking.Payment.Amount;

                var refundResult = await _paymentProcessor.RefundPaymentAsync(
                    booking.Payment.TapChargeId,
                    refundAmount,
                    booking.Payment.Currency,
                    request.Reason);

                if (refundResult.IsSuccessful)
                {
                    booking.Payment.Status = refundAmount == booking.Payment.Amount ? "Refunded" : "PartiallyRefunded";
                    booking.Payment.RefundedAmount = (booking.Payment.RefundedAmount ?? 0) + refundResult.RefundedAmount;
                    booking.Payment.RefundedAt = DateTime.UtcNow;

                    // Send refund confirmation
                    await _notificationService.SendRefundConfirmationAsync(
                        booking.User.Email,
                        booking.Id,
                        refundResult.RefundedAmount,
                        booking.Payment.Currency);
                }

                await _bookingRepository.UpdateAsync(booking);
                await _bookingRepository.SaveChangesAsync();

                return refundResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for booking {request.BookingId}");
                throw;
            }
        }
    }
}
