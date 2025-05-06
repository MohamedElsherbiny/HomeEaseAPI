using System;
using MediatR;
using Massage.Application.DTOs;
using Massage.Application.Commands;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Massage.Application.Exceptions;
using Massage.Domain.Enums;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;

namespace Massage.Application.Commands
{
    public class CreateBookingCommand : IRequest<Guid>
    {
        public CreateBookingRequestDto BookingRequest { get; set; }
        public Guid UserId { get; set; }
    }

    public class UpdateBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public UpdateBookingRequestDto UpdateRequest { get; set; }
    }

    public class ConfirmBookingCommand : IRequest<bool>
    {
        public BookingConfirmationDto ConfirmationRequest { get; set; }
        public Guid ProviderId { get; set; }
    }

    public class CancelBookingCommand : IRequest<bool>
    {
        public BookingCancellationDto CancellationRequest { get; set; }
        public Guid UserId { get; set; }
        public bool IsProvider { get; set; }
    }

    public class ProcessBookingPaymentCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public PaymentInfoDto PaymentInfo { get; set; }
    }
}


// COMMAND HANDLERS

namespace Massage.Application.Handlers.CommandHandlers
{
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

    public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<UpdateBookingCommandHandler> _logger;

        public UpdateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            ILogger<UpdateBookingCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
                if (booking == null)
                    throw new EntityNotFoundException("Booking not found");

                // Validate that the user owns this booking
                if (booking.UserId != request.UserId)
                    throw new UnauthorizedAccessException("You are not authorized to update this booking");

                // Validate that the booking can be updated (not completed or cancelled)
                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                    throw new BusinessRuleException("Cannot update a completed or cancelled booking");

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
                        throw new BusinessRuleException("Provider is not available at the selected time");

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

    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CancelBookingCommandHandler> _logger;

        public CancelBookingCommandHandler(
            IBookingRepository bookingRepository,
            INotificationService notificationService,
            ILogger<CancelBookingCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(request.CancellationRequest.BookingId);
                if (booking == null)
                    throw new EntityNotFoundException("Booking not found");

                // Validate authorization based on who is cancelling
                if (request.IsProvider)
                {
                    if (booking.ProviderId != request.UserId)
                        throw new UnauthorizedAccessException("You are not authorized to cancel this booking");
                }
                else
                {
                    if (booking.UserId != request.UserId)
                        throw new UnauthorizedAccessException("You are not authorized to cancel this booking");
                }

                // Validate that the booking can be cancelled (not completed or already cancelled)
                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                    throw new BusinessRuleException("Cannot cancel a completed or already cancelled booking");

                // Calculate if cancellation fees apply
                var cancellationTimeframe = booking.AppointmentDateTime - DateTime.UtcNow;
                bool cancellationFeeApplies = cancellationTimeframe.TotalHours < 24;

                // Update booking
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = request.CancellationRequest.CancellationReason;

                // Handle payment status if applicable
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
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling booking {request.CancellationRequest.BookingId}");
                throw;
            }
        }
    }

    public class ProcessBookingPaymentCommandHandler : IRequestHandler<ProcessBookingPaymentCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly ILogger<ProcessBookingPaymentCommandHandler> _logger;

        public ProcessBookingPaymentCommandHandler(
            IBookingRepository bookingRepository,
            IPaymentProcessor paymentProcessor,
            ILogger<ProcessBookingPaymentCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _paymentProcessor = paymentProcessor;
            _logger = logger;
        }

        public async Task<bool> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
                if (booking == null)
                    throw new EntityNotFoundException("Booking not found");

                // Ensure booking has a payment record
                if (booking.Payment == null)
                {
                    booking.Payment = new PaymentInfo
                    {
                        Amount = request.PaymentInfo.Amount,
                        Currency = request.PaymentInfo.Currency,
                        PaymentMethod = request.PaymentInfo.PaymentMethod,
                        Status = "Processing"
                    };
                }
                else
                {
                    booking.Payment.Amount = request.PaymentInfo.Amount;
                    booking.Payment.Currency = request.PaymentInfo.Currency;
                    booking.Payment.PaymentMethod = request.PaymentInfo.PaymentMethod;
                    booking.Payment.Status = "Processing";
                }

                // Process payment through payment gateway
                var paymentResult = await _paymentProcessor.ProcessPaymentAsync(
                    booking.Id,
                    booking.Payment.Amount,
                    booking.Payment.Currency,
                    booking.Payment.PaymentMethod,
                    request.PaymentInfo.TransactionId);

                if (paymentResult.IsSuccessful)
                {
                    booking.Payment.Status = "Completed";
                    booking.Payment.TransactionId = paymentResult.TransactionId;
                    booking.Payment.ProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    booking.Payment.Status = "Failed";
                    _logger.LogWarning($"Payment failed for booking {booking.Id}: {paymentResult.ErrorMessage}");
                }

                await _bookingRepository.UpdateAsync(booking);
                await _bookingRepository.SaveChangesAsync();

                return paymentResult.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment for booking {request.BookingId}");
                throw;
            }
        }
    }
}

