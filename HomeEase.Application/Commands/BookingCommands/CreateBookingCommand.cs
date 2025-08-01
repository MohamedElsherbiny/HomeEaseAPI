using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Exceptions;
using HomeEase.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeEase.Application.Commands.BookingCommands;

public class CreateBookingCommand : IRequest<Guid>
{
    public CreateBookingRequestDto BookingRequest { get; set; }
    public Guid UserId { get; set; }
}

public class CreateBookingCommandHandler(
    IBookingRepository _bookingRepository,
    IUserRepository _userRepository,
    IProviderRepository _providerRepository,
    IServiceRepository _serviceRepository, 
    ILogger<CreateBookingCommandHandler> _logger) : IRequestHandler<CreateBookingCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that the user exists
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                throw new BusinessException("User not found");
            }

            // Validate that the provider exists
            var provider = await _providerRepository.GetByIdAsync(request.BookingRequest.ProviderId);
            if (provider == null)
            {
                throw new BusinessException("Provider not found");
            }

            // Validate that the service exists and belongs to the provider
            var service = await _serviceRepository.GetByIdAsync(request.BookingRequest.ServiceId);
            if (service == null || service.ProviderId != request.BookingRequest.ProviderId)
            {
                throw new BusinessException("Service not found or does not belong to the provider");
            }

            // Validate customer address for home service
            if (request.BookingRequest.IsHomeService && string.IsNullOrWhiteSpace(request.BookingRequest.CustomerAddress))
            {
                throw new BusinessException("Customer address is required for home service");
            }

            // Combine date and time for appointment datetime
            var appointmentDateTime = request.BookingRequest.AppointmentDate.Date + request.BookingRequest.AppointmentTime;

            // Validate appointment is in the future
            if (appointmentDateTime <= DateTime.Now)
            {
                throw new BusinessException("Appointment time must be in the future");
            }

            // Check provider availability
            var isAvailable = await _providerRepository.CheckAvailabilityAsync(
                provider,
                appointmentDateTime);

            if (!isAvailable)
            {
                throw new BusinessException("Provider is not available at the selected time");
            }

            // Create new booking
            var maxSerialNumber = await _bookingRepository.GetMaxSerialNumberAsync();
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                SerialNumber = maxSerialNumber + 1,
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
            return booking.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            throw;
        }
    }
}