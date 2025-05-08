using Massage.Application.Commands.BookingCommands;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Enums;
using Massage.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.BookingCommands
{
    public class CreateBookingCommand : IRequest<Guid>
    {
        public CreateBookingRequestDto BookingRequest { get; set; }
        public Guid UserId { get; set; }
    }
}


// COMMAND HANDLER
public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IProviderRepository providerRepository,
        IServiceRepository serviceRepository,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that the user exists
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new EntityNotFoundException("User not found");

            // Validate that the provider exists
            var provider = await _providerRepository.GetByIdAsync(request.BookingRequest.ProviderId);
            if (provider == null)
                throw new EntityNotFoundException("Provider not found");

            // Validate that the service exists and belongs to the provider
            var service = await _serviceRepository.GetByIdAsync(request.BookingRequest.ServiceId);
            if (service == null || service.ProviderId != request.BookingRequest.ProviderId)
                throw new EntityNotFoundException("Service not found or does not belong to the provider");

            // Check provider availability
            var isAvailable = await _providerRepository.CheckAvailabilityAsync(
                request.BookingRequest.ProviderId,
                request.BookingRequest.AppointmentDateTime,
                service.DurationMinutes);

            if (!isAvailable)
                throw new BusinessRuleException("Provider is not available at the selected time");

            // Create new booking
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ProviderId = request.BookingRequest.ProviderId,
                ServiceId = request.BookingRequest.ServiceId,
                AppointmentDateTime = request.BookingRequest.AppointmentDateTime,
                Notes = request.BookingRequest.Notes,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                DurationMinutes = service.DurationMinutes,
                ServicePrice = service.Price
            };

            // Set location if provided
            if (request.BookingRequest.Location != null)
            {
                booking.Location.Address = new Address
                {
                    Street = request.BookingRequest.Location.Street,
                    City = request.BookingRequest.Location.City,
                    State = request.BookingRequest.Location.State,
                    ZipCode = request.BookingRequest.Location.ZipCode,
                    Country = request.BookingRequest.Location.Country
                };
            }

            // Create initial payment record if payment info is provided
            if (request.BookingRequest.PaymentInfo != null)
            {
                booking.Payment = new PaymentInfo
                {
                    Amount = request.BookingRequest.PaymentInfo.Amount,
                    Currency = request.BookingRequest.PaymentInfo.Currency,
                    PaymentMethod = request.BookingRequest.PaymentInfo.PaymentMethod,
                    Status = "Pending"
                };
            }

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation($"Booking created successfully. ID: {booking.Id}");
            return booking.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            throw;
        }
    }
}