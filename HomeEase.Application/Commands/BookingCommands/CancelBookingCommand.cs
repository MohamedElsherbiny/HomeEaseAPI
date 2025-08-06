using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using HomeEase.Resources;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class CancelBookingCommand : IRequest<EntityResult>
{
    public BookingCancellationDto? CancellationRequest { get; set; }
    public Guid UserId { get; set; }
    public bool IsProvider { get; set; }
}

public class CancelBookingCommandHandler(
    IBookingRepository _bookingRepository,
    INotificationService _notificationService,
    ILogger<CancelBookingCommandHandler> _logger) : IRequestHandler<CancelBookingCommand, EntityResult>
{
    public async Task<EntityResult> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.CancellationRequest.BookingId);
        if (booking == null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.BookingNotFound), Messages.BookingNotFound));
        }

        // Validate authorization based on who is cancelling
        if (request.IsProvider)
        {
            if (booking.ProviderId != request.UserId)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.UnauthorizedBookingCancellation), Messages.UnauthorizedBookingCancellation));
            }
        }
        else
        {
            if (booking.UserId != request.UserId)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.UnauthorizedBookingCancellation), Messages.UnauthorizedBookingCancellation));
            }
        }

        // Validate that the booking can be cancelled (not completed or already cancelled)
        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.BookingCancellationNotAllowed), Messages.BookingCancellationNotAllowed));
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

        return EntityResult.Success;


    }
}