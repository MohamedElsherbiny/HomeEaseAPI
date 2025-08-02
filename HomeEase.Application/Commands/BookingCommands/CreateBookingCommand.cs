using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class CreateBookingCommand : IRequest<EntityResult>
{
    public CreateBookingRequestDto BookingRequest { get; set; }
    public Guid UserId { get; set; }
}

public class CreateBookingCommandHandler(
    IBookingRepository _bookingRepository,
    IUserRepository _userRepository,
    IProviderRepository _providerRepository,
    IServiceRepository _serviceRepository,
    ILogger<CreateBookingCommandHandler> _logger) : IRequestHandler<CreateBookingCommand, EntityResult>
{
    public async Task<EntityResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFound), string.Format(Messages.UserNotFound, request.UserId.ToString())));
        }

        var provider = await _providerRepository.GetByIdAsync(request.BookingRequest.ProviderId);
        if (provider is null)
        {
            return EntityResult.Failed(new EntityError(
                   nameof(Messages.ProviderNotFound),
                   string.Format(Messages.ProviderNotFound, request.BookingRequest.ProviderId)));
        }

        var service = await _serviceRepository.GetByIdAsync(request.BookingRequest.ServiceId);
        if (service == null || service.ProviderId != request.BookingRequest.ProviderId)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ServiceNotFound), Messages.ServiceNotFound));
        }

        if (request.BookingRequest.IsHomeService && string.IsNullOrWhiteSpace(request.BookingRequest.CustomerAddress))
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.CustomerAddressRequiredForHomeService), Messages.CustomerAddressRequiredForHomeService));
        }

        var appointmentDateTime = request.BookingRequest.AppointmentDate.Date + request.BookingRequest.AppointmentTime;

        if (appointmentDateTime <= DateTime.Now)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.AppointmentTimeMustBeFuture), Messages.AppointmentTimeMustBeFuture));
        }

        var isAvailable = await _providerRepository.CheckAvailabilityAsync(
            provider,
            appointmentDateTime);

        if (!isAvailable)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ProviderNotAvailableAtThisTime), Messages.ProviderNotAvailableAtThisTime));
        }

        var today = DateTime.UtcNow.Date;
        var bookingCount = await _bookingRepository.GetBookingCountByDateAsync(today);

        var serialNumber = $"B{today:yyyyMMdd}{(bookingCount + 1):D4}"; //: B202508010001
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            SerialNumber = serialNumber,
            UserId = request.UserId,
            ProviderId = request.BookingRequest.ProviderId,
            ServiceId = request.BookingRequest.ServiceId,
            CustomerAddress = request.BookingRequest.CustomerAddress ?? "",
            AppointmentDate = request.BookingRequest.AppointmentDate,
            AppointmentTime = request.BookingRequest.AppointmentTime,
            AppointmentDateTime = appointmentDateTime,
            IsHomeService = request.BookingRequest.IsHomeService,
            Notes = request.BookingRequest.Notes ?? "",
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            DurationMinutes = request.BookingRequest.DurationMinutes,
            ServicePrice = service.Price,
            ServiceHomePrice = service.HomePrice
        };

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        _logger.LogInformation($"Booking created successfully. ID: {booking.Id}, Type: {(booking.IsHomeService ? "Home Service" : "Center Visit")}");

        return EntityResult.SuccessWithData(new { bookingId = booking.Id });

    }
}