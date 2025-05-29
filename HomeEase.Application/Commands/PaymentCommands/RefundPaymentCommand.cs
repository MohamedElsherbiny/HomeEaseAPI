using AutoMapper;
using HomeEase.Application.Commands.PaymentCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.PaymentCommands
{
    public class RefundPaymentCommand : IRequest<PaymentResultDto>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}


public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, PaymentResultDto>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IPaymentService _paymentService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefundPaymentCommandHandler(
        IPaymentInfoRepository paymentInfoRepository,
        IPaymentService paymentService,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _paymentService = paymentService;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaymentResultDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentInfo = await _paymentInfoRepository.GetByIdAsync(request.Id);
        if (paymentInfo == null)
            throw new ApplicationException("Payment not found.");

        var booking = await _bookingRepository.GetByIdAsync(paymentInfo.BookingId);
        if (booking == null)
            throw new ApplicationException("Booking not found.");

        if (booking.UserId != request.UserId)
            throw new ApplicationException("User is not authorized to refund this payment.");

        if (paymentInfo.Status != "Completed")
            throw new ApplicationException("Only completed payments can be refunded.");

        var refundResult = await _paymentService.RefundPaymentAsync(paymentInfo);
        if (refundResult.IsSuccessful)
        {
            paymentInfo.Status = "Refunded";
            paymentInfo.ProcessedAt = DateTime.UtcNow;
            _paymentInfoRepository.Update(paymentInfo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<PaymentResultDto>(refundResult);
    }
}