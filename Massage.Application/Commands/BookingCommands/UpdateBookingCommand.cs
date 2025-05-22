using Massage.Application.Commands.BookingCommands;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Repos;
using Massage.Domain.Entities;
using Massage.Domain.Enums;
using Massage.Domain.Exceptions;
using Massage.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Commands.BookingCommands;

public class UpdateBookingCommand : IRequest<bool>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public UpdateBookingRequestDto UpdateRequest { get; set; }
}

public class UpdateBookingCommandHandler(
    IBookingRepository _bookingRepository,
    IServiceRepository _serviceRepository,
    ILogger<UpdateBookingCommandHandler> _logger) : IRequestHandler<UpdateBookingCommand, bool>
{
    public async Task<bool> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new BusinessException("Booking not found");
            }

            // Validate that the user owns this booking
            if (booking.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this booking");
            }

            // Validate that the booking can be updated (not completed or cancelled)
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                throw new BusinessException("Cannot update a completed or cancelled booking");
            }

            // Update appointment date/time if provided
            if (request.UpdateRequest.AppointmentDateTime.HasValue)
            {
                // Check provider availability for the new time
                var service = await _serviceRepository.GetByIdAsync(booking.ServiceId);
                var isAvailable = await _bookingRepository.CheckProviderAvailabilityAsync(
                    booking.ProviderId,
                    request.UpdateRequest.AppointmentDateTime.Value,
                    service.DurationMinutes,
                    booking.Id); // Exclude current booking from availability check

                if (!isAvailable)
                    throw new BusinessException("Provider is not available at the selected time");

                booking.AppointmentDateTime = request.UpdateRequest.AppointmentDateTime.Value;
            }

            // Update notes if provided
            if (request.UpdateRequest.Notes != null)
            {
                booking.Notes = request.UpdateRequest.Notes;
            }

            // Update location if provided
            if (request.UpdateRequest.Location != null)
            {
                booking.Location.Address = new Address
                {
                    Street = request.UpdateRequest.Location.Street,
                    City = request.UpdateRequest.Location.City,
                    State = request.UpdateRequest.Location.State,
                    ZipCode = request.UpdateRequest.Location.ZipCode,
                    Country = request.UpdateRequest.Location.Country
                };
            }

            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation($"Booking updated successfully. ID: {booking.Id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating booking {request.BookingId}");
            throw;
        }
    }
}