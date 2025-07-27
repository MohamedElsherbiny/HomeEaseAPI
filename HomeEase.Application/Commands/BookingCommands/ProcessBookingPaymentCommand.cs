using HomeEase.Application.DTOs;
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

public class ProcessBookingPaymentCommandHandler : IRequestHandler<ProcessBookingPaymentCommand, PaymentResult>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProcessBookingPaymentCommandHandler> _logger;

    public ProcessBookingPaymentCommandHandler(
        IBookingRepository bookingRepository,
        IPaymentProcessor paymentProcessor,
        INotificationService notificationService,
        ILogger<ProcessBookingPaymentCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _paymentProcessor = paymentProcessor;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PaymentResult> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found");

            // Initialize or update payment record
            if (booking.Payment == null)
            {
                booking.Payment = new PaymentInfo
                {
                    Id = Guid.NewGuid(),
                    BookingId = request.BookingId,
                    Amount = request.PaymentInfo.Amount,
                    Currency = request.PaymentInfo.Currency ?? "SAR",
                    PaymentMethod = request.PaymentInfo.PaymentMethod,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
            }
            else
            {
                booking.Payment.Amount = request.PaymentInfo.Amount;
                booking.Payment.Currency = request.PaymentInfo.Currency ?? "SAR";
                booking.Payment.PaymentMethod = request.PaymentInfo.PaymentMethod;
                booking.Payment.Status = "Processing";
            }

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
            booking.Payment.TapChargeId = paymentResult.TapChargeId;
            booking.Payment.TransactionId = paymentResult.TransactionId;
            booking.Payment.PaymentUrl = paymentResult.PaymentUrl;
            booking.Payment.ErrorCode = paymentResult.ErrorCode;
            booking.Payment.ErrorMessage = paymentResult.ErrorMessage;

            if (paymentResult.IsSuccessful)
            {
                booking.Payment.ProcessedAt = DateTime.UtcNow;
                _logger.LogInformation($"Payment completed successfully for booking {booking.Id}");
            }
            else
            {
                _logger.LogWarning($"Payment failed for booking {booking.Id}: {paymentResult.ErrorMessage}");
            }

            // Single save operation after all modifications
            try
            {
                await _bookingRepository.UpdatePaymentAsync(booking.Payment);
                await _bookingRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency conflict while updating booking {booking.Id}");

                throw new BusinessException("Your reservation has been modified by someone else. Please refresh the page and try again.");
            }


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