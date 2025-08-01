using HomeEase.Application.Commands.BookingCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class ConfirmBookingCommand : IRequest<bool>
{
    public BookingConfirmationDto ConfirmationRequest { get; set; }
    public Guid ProviderId { get; set; }
}

public class ConfirmBookingCommandHandler(
    IBookingRepository _bookingRepository,
    INotificationService _notificationService,
    ILogger<ConfirmBookingCommandHandler> _logger) : IRequestHandler<ConfirmBookingCommand, bool>
{

    public async Task<bool> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.ConfirmationRequest.BookingId);
            if (booking == null)
            {
                throw new BusinessException("Booking not found");
            }

            // Validate that the provider owns this booking
            if (booking.ProviderId != request.ProviderId)
            {
                throw new UnauthorizedAccessException("You are not authorized to confirm this booking");
            }

            // Validate that the booking is in a Pending state
            if (booking.Status != BookingStatus.Pending)
            {
                throw new BusinessException("Only pending bookings can be confirmed");
            }

            if (request.ConfirmationRequest.IsConfirmed)
            {
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedAt = DateTime.UtcNow;


                // Send notification to user
                await _notificationService.SendBookingConfirmationAsync(booking);
            }
            else
            {
                // If not confirmed, treat it as rejection
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                // Send notification to user
                await _notificationService.SendBookingRejectionAsync(booking);
            }

            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation($"Booking {(request.ConfirmationRequest.IsConfirmed ? "confirmed" : "rejected")}. ID: {booking.Id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error confirming booking {request.ConfirmationRequest.BookingId}");
            throw;
        }
    }
}