using Massage.Application.Commands.BookingCommands;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.BookingCommands
{
    public class ConfirmBookingCommand : IRequest<bool>
    {
        public BookingConfirmationDto ConfirmationRequest { get; set; }
        public Guid ProviderId { get; set; }
    }
}


// COMMAND HANDLER
public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, bool>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;

    public ConfirmBookingCommandHandler(
        IBookingRepository bookingRepository,
        INotificationService notificationService,
        ILogger<ConfirmBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.ConfirmationRequest.BookingId);
            if (booking == null)
                throw new EntityNotFoundException("Booking not found");

            // Validate that the provider owns this booking
            if (booking.ProviderId != request.ProviderId)
                throw new UnauthorizedAccessException("You are not authorized to confirm this booking");

            // Validate that the booking is in a Pending state
            if (booking.Status != BookingStatus.Pending)
                throw new BusinessRuleException("Only pending bookings can be confirmed");

            if (request.ConfirmationRequest.IsConfirmed)
            {
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedAt = DateTime.UtcNow;

                // Add provider notes if any
                if (!string.IsNullOrEmpty(request.ConfirmationRequest.Notes))
                {
                    booking.Notes += $"\n[Provider Note: {request.ConfirmationRequest.Notes}]";
                }

                // Send notification to user
                await _notificationService.SendBookingConfirmationAsync(booking);
            }
            else
            {
                // If not confirmed, treat it as rejection
                booking.Status = BookingStatus.Rejected;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = request.ConfirmationRequest.Notes ?? "Rejected by provider";

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