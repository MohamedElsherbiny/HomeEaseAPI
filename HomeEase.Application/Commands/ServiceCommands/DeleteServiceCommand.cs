using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.ServiceCommands
{
    public class DeleteServiceCommand : IRequest<bool>
    {
        public Guid ServiceId { get; set; }
    }
}


public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
        if (service == null)
            return false;

        _serviceRepository.Delete(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}