using Massage.Application.DTOs;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Enums;
using Massage.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Commands.BookingCommands;

public class CancelBookingCommand : IRequest<bool>
{
    public BookingCancellationDto? CancellationRequest { get; set; }
    public Guid UserId { get; set; }
    public bool IsProvider { get; set; }
}

public class CancelBookingCommandHandler(
    IBookingRepository _bookingRepository,
    INotificationService _notificationService,
    ILogger<CancelBookingCommandHandler> _logger) : IRequestHandler<CancelBookingCommand, bool>
{
    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.CancellationRequest.BookingId);
            if (booking == null)
            {
                throw new BusinessException("Booking not found");
            }

            // Validate authorization based on who is cancelling
            if (request.IsProvider)
            {
                if (booking.ProviderId != request.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to cancel this booking");
                }
            }
            else
            {
                if (booking.UserId != request.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to cancel this booking");
                }
            }

            // Validate that the booking can be cancelled (not completed or already cancelled)
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                throw new BusinessException("Cannot cancel a completed or already cancelled booking");
            }

            var cancellationTimeframe = booking.AppointmentDateTime - DateTime.UtcNow;
            bool cancellationFeeApplies = cancellationTimeframe.TotalHours < 24;

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = request.CancellationRequest.CancellationReason;

            if (booking.Payment != null)
            {
                booking.Payment.Status = cancellationFeeApplies ? "Partial Refund" : "Refunded";
            }

            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            // Send notifications
            if (request.IsProvider)
            {
                await _notificationService.SendProviderCancellationAsync(booking);
            }
            else
            {
                await _notificationService.SendUserCancellationAsync(booking);
            }

            _logger.LogInformation($"Booking cancelled successfully. ID: {booking.Id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling booking {request.CancellationRequest.BookingId}");
            throw;
        }
    }
}