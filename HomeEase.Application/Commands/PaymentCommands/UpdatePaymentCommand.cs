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
    public class UpdatePaymentCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public UpdatePaymentDto PaymentDto { get; set; }
    }
}


public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, bool>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePaymentCommandHandler(
        IPaymentInfoRepository paymentInfoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentInfo = await _paymentInfoRepository.GetByIdAsync(request.Id);
        if (paymentInfo == null)
            return false;

        // Validate status transitions
        if (!IsValidStatusTransition(paymentInfo.Status, request.PaymentDto.Status))
            throw new ApplicationException($"Invalid status transition from {paymentInfo.Status} to {request.PaymentDto.Status}.");

        _mapper.Map(request.PaymentDto, paymentInfo);
        _paymentInfoRepository.Update(paymentInfo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        // Define valid transitions
        return (currentStatus, newStatus) switch
        {
            ("Pending", "Completed") => true,
            ("Pending", "Failed") => true,
            ("Completed", "Refunded") => true,
            _ => false
        };
    }
}