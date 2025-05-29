using AutoMapper;
using HomeEase.Application.Commands.PaymentCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.PaymentCommands
{
    public class CreatePaymentCommand : IRequest<PaymentResultDto>
    {
        public Guid UserId { get; set; }
        public CreatePaymentDto PaymentDto { get; set; }
    }
}


public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResultDto>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePaymentCommandHandler(
        IPaymentInfoRepository paymentInfoRepository,
        IBookingRepository bookingRepository,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _bookingRepository = bookingRepository;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaymentResultDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.PaymentDto.BookingId);
        if (booking == null)
            throw new ApplicationException("Booking not found.");

        if (booking.UserId != request.UserId)
            throw new ApplicationException("User is not authorized to pay for this booking.");

        if (booking.Status != BookingStatus.Confirmed) // Adjust based on your BookingStatus enum
            throw new ApplicationException("Booking must be confirmed to process payment.");

        var paymentInfo = _mapper.Map<PaymentInfo>(request.PaymentDto);
        paymentInfo.Id = Guid.NewGuid();
        paymentInfo.Status = "Pending";
        paymentInfo.Amount = booking.TotalAmount;
        paymentInfo.Currency = booking.Currency ?? "SAR";
        paymentInfo.TransactionId = string.Empty;

        await _paymentInfoRepository.AddAsync(paymentInfo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var paymentResult = await _paymentService.ProcessPaymentAsync(paymentInfo, request.PaymentDto.PaymentToken);
        paymentInfo.Status = paymentResult.IsSuccessful ? "Completed" : "Failed";
        paymentInfo.TransactionId = paymentResult.TransactionId;
        paymentInfo.ProcessedAt = paymentResult.IsSuccessful ? DateTime.UtcNow : null;

        _paymentInfoRepository.Update(paymentInfo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PaymentResultDto>(paymentResult);
    }
}