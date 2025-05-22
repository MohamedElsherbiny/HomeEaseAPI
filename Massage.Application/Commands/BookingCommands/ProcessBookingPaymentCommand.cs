using Massage.Application.DTOs;
using Massage.Application.Interfaces.Repos;
using Massage.Domain.Entities;
using Massage.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Commands.BookingCommands;

public class ProcessBookingPaymentCommand : IRequest<bool>
{
    public Guid BookingId { get; set; }
    public PaymentInfoDto PaymentInfo { get; set; }
}

public class ProcessBookingPaymentCommandHandler(
    IBookingRepository _bookingRepository,
    IPaymentProcessor _paymentProcessor,
    ILogger<ProcessBookingPaymentCommandHandler> _logger) : IRequestHandler<ProcessBookingPaymentCommand, bool>
{
    public async Task<bool> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found");

            // Ensure booking has a payment record
            if (booking.Payment == null)
            {
                booking.Payment = new PaymentInfo
                {
                    Amount = request.PaymentInfo.Amount,
                    Currency = request.PaymentInfo.Currency,
                    PaymentMethod = request.PaymentInfo.PaymentMethod,
                    Status = "Processing"
                };
            }
            else
            {
                booking.Payment.Amount = request.PaymentInfo.Amount;
                booking.Payment.Currency = request.PaymentInfo.Currency;
                booking.Payment.PaymentMethod = request.PaymentInfo.PaymentMethod;
                booking.Payment.Status = "Processing";
            }

            // Process payment through payment gateway
            var paymentResult = await _paymentProcessor.ProcessPaymentAsync(
                booking.Id,
                booking.Payment.Amount,
                booking.Payment.Currency,
                booking.Payment.PaymentMethod,
                request.PaymentInfo.TransactionId);

            if (paymentResult.IsSuccessful)
            {
                booking.Payment.Status = "Completed";
                booking.Payment.TransactionId = paymentResult.TransactionId;
                booking.Payment.ProcessedAt = DateTime.UtcNow;
            }
            else
            {
                booking.Payment.Status = "Failed";
                _logger.LogWarning($"Payment failed for booking {booking.Id}: {paymentResult.ErrorMessage}");
            }

            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            return paymentResult.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing payment for booking {request.BookingId}");
            throw;
        }
    }
}