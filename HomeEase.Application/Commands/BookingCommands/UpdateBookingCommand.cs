using HomeEase.Application.Commands.BookingCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Exceptions;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class UpdateBookingCommand : IRequest<EntityResult>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public UpdateBookingRequestDto UpdateRequest { get; set; }
}

public class UpdateBookingCommandHandler(
    IBookingRepository _bookingRepository,
    IServiceRepository _serviceRepository,
    ILogger<UpdateBookingCommandHandler> _logger) : IRequestHandler<UpdateBookingCommand, EntityResult>
{
    public async Task<EntityResult> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.BookingNotFound), Messages.BookingNotFound));
        }

        if (booking.UserId != request.UserId)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UnauthorizedBookingUpdate), Messages.UnauthorizedBookingUpdate));
        }

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.BookingUpdateNotAllowed), Messages.BookingUpdateNotAllowed));
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
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.ProviderUnavailable), Messages.ProviderUnavailable));
            }

            booking.AppointmentDateTime = request.UpdateRequest.AppointmentDateTime.Value;
        }

        // Update notes if provided
        if (request.UpdateRequest.Notes != null)
        {
            booking.Notes = request.UpdateRequest.Notes;
        }

        // Update location if provided
        if (request.UpdateRequest.Address != null)
        {
            booking.Address = new Address
            {
                Street = request.UpdateRequest.Address.Street,
                City = request.UpdateRequest.Address.City,
                State = request.UpdateRequest.Address.State,
                ZipCode = request.UpdateRequest.Address.ZipCode,
                Country = request.UpdateRequest.Address.Country
            };
        }

        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        _logger.LogInformation($"Booking updated successfully. ID: {booking.Id}");

        return EntityResult.Success;
    }
}