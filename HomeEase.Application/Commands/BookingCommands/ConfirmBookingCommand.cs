using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using HomeEase.Resources;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class ConfirmBookingCommand : IRequest<EntityResult>
{
    public BookingConfirmationDto ConfirmationRequest { get; set; }
    public Guid ProviderId { get; set; }
}

public class ConfirmBookingCommandHandler(
    IBookingRepository _bookingRepository,
    INotificationService _notificationService,
    ILogger<ConfirmBookingCommandHandler> _logger) : IRequestHandler<ConfirmBookingCommand, EntityResult>
{

    public async Task<EntityResult> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.ConfirmationRequest.BookingId);
        if (booking == null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.BookingNotFound), Messages.BookingNotFound));
        }

        if (booking.ProviderId != request.ProviderId)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UnauthorizedBookingCancellation), Messages.UnauthorizedBookingCancellation));
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.BookingMustBePending), Messages.BookingMustBePending));
        }

        if (request.ConfirmationRequest.IsConfirmed)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;

            await _notificationService.SendBookingConfirmationAsync(booking);
        }
        else
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            await _notificationService.SendBookingRejectionAsync(booking);
        }

        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        return EntityResult.Success;

    }
}