using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class ProcessBookingPaymentCommand : IRequest<PaymentResult>
{
    public Guid BookingId { get; set; }
    public PaymentInfoDto PaymentInfo { get; set; }
    public CustomerInfo Customer { get; set; }
}

public class ProcessBookingPaymentCommandHandler(
    IBookingRepository _bookingRepository,
    IPaymentProcessor _paymentProcessor,
    INotificationService _notificationService,
    IAppDbContext _context,
    ILogger<ProcessBookingPaymentCommandHandler> _logger) : IRequestHandler<ProcessBookingPaymentCommand, PaymentResult>
{

    public async Task<PaymentResult> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                throw new BusinessException("Booking not found");

            // Initialize or update payment record
            if (booking.Payment == null)
            {
                booking.Payment = new PaymentInfo
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    Booking = booking,
                    Amount = request.PaymentInfo.Amount,
                    Currency = request.PaymentInfo.Currency ?? "SAR",
                    PaymentMethod = request.PaymentInfo.PaymentMethod,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentInfos.Add(booking.Payment);

            }
            else
            {
                booking.Payment.Amount = request.PaymentInfo.Amount;
                booking.Payment.Currency = request.PaymentInfo.Currency ?? "SAR";
                booking.Payment.PaymentMethod = request.PaymentInfo.PaymentMethod;
                booking.Payment.Status = "Processing";

                _context.PaymentInfos.Update(booking.Payment);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Process payment through Tap Gateway
            var paymentResult = await _paymentProcessor.ProcessPaymentAsync(
                booking.Id,
                booking.Payment.Amount,
                booking.Payment.Currency,
                booking.Payment.PaymentMethod,
                request.Customer,
                request.PaymentInfo.TransactionId);

            // Update payment record based on result
            booking.Payment.Status = paymentResult.Status.ToString();
            booking.Payment.TransactionId = paymentResult.TransactionId ?? "";
            booking.Payment.PaymentUrl = paymentResult.PaymentUrl ?? "";
            booking.Payment.ErrorCode = paymentResult.ErrorCode ?? "";
            booking.Payment.ErrorMessage = paymentResult.ErrorMessage ?? "";

            if (paymentResult.IsSuccessful)
            {
                booking.Payment.ProcessedAt = DateTime.UtcNow;
                _logger.LogInformation($"Payment completed successfully for booking {booking.Id}");
            }
            else
            {
                _logger.LogWarning($"Payment failed for booking {booking.Id}: {paymentResult.ErrorMessage}");
            }

            _context.PaymentInfos.Update(booking.Payment);
            await _context.SaveChangesAsync(cancellationToken);

            // Send notifications after successful save
            if (paymentResult.IsSuccessful)
            {
                await _notificationService.SendPaymentConfirmationAsync(
                    request.Customer.Email,
                    booking.Id,
                    booking.Payment.Amount,
                    booking.Payment.Currency);
            }
            else
            {
                await _notificationService.SendPaymentFailureNotificationAsync(
                    request.Customer.Email,
                    booking.Id,
                    paymentResult.ErrorMessage);
            }

            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing payment for booking {request.BookingId}");
            throw;
        }
    }
}