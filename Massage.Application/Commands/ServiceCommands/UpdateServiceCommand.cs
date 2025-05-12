using Massage.Application.Commands.ServiceCommands;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Enums;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.ServiceCommands
{
    public class UpdateServiceCommand : IRequest<bool>
    {
        public Guid ServiceId { get; set; }
        public UpdateServiceDto ServiceDto { get; set; }
    }
}


public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, bool>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
        if (service == null)
            return false;

        if (request.ServiceDto.Name != null)
            service.Name = request.ServiceDto.Name;

        if (request.ServiceDto.Description != null)
            service.Description = request.ServiceDto.Description;

        if (request.ServiceDto.Price.HasValue)
            service.Price = request.ServiceDto.Price.Value;

        if (request.ServiceDto.DurationMinutes.HasValue)
            service.DurationMinutes = request.ServiceDto.DurationMinutes.Value;

        //if (request.ServiceDto.ServiceType != null)
        //    service.ServiceType = Enum.Parse<ServiceType>(request.ServiceDto.ServiceType);


        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}