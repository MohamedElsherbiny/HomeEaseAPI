using HomeEase.Application.Commands.PaymentCommands;
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
    public class DeletePaymentCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}


public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, bool>
{
    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePaymentCommandHandler(IPaymentInfoRepository paymentInfoRepository, IUnitOfWork unitOfWork)
    {
        _paymentInfoRepository = paymentInfoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentInfo = await _paymentInfoRepository.GetByIdAsync(request.Id);
        if (paymentInfo == null)
            return false;

        _paymentInfoRepository.Delete(paymentInfo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}