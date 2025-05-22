using Massage.Application.DTOs;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Enums;
using Massage.Domain.Exceptions;
using Massage.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Massage.Application.Commands.BookingCommands;

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

            // Check provider availability
            var isAvailable = await _providerRepository.CheckAvailabilityAsync(
                request.BookingRequest.ProviderId,
                request.BookingRequest.AppointmentDateTime,
                service.DurationMinutes);

            if (!isAvailable)
            {
                throw new BusinessException("Provider is not available at the selected time");
            }

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

            //// Set location if provided
            //if (request.BookingRequest.Location != null)
            //{
            //    booking.Location = new Location
            //    {
            //        Address = new Address
            //        {
            //            Street = request.BookingRequest.Location.Street,
            //            City = request.BookingRequest.Location.City,
            //            State = request.BookingRequest.Location.State,
            //            ZipCode = request.BookingRequest.Location.ZipCode,
            //            Country = request.BookingRequest.Location.Country
            //        }
            //    };
            //}

            // Create initial payment record if payment info is provided
            //if (request.BookingRequest.PaymentInfo != null)
            //{
            //    booking.Payment = new PaymentInfo
            //    {
            //        Amount = request.BookingRequest.PaymentInfo.Amount,
            //        Currency = request.BookingRequest.PaymentInfo.Currency,
            //        PaymentMethod = request.BookingRequest.PaymentInfo.PaymentMethod,
            //        Status = "Pending"
            //    };
            //}

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