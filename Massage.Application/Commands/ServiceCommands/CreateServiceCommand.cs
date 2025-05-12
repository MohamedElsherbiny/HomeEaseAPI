using Massage.Application.Commands.ServiceCommands;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
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
    public class CreateServiceCommand : IRequest<Guid>
    {
        public Guid ProviderId { get; set; }
        public CreateServiceDto ServiceDto { get; set; }
    }
}


public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(
        IProviderRepository providerRepository,
        IServiceRepository serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
            throw new ApplicationException($"Provider with ID {request.ProviderId} not found");

        var service = new Service
        {
            Id = Guid.NewGuid(),
            ProviderId = request.ProviderId,
            Name = request.ServiceDto.Name,
            Description = request.ServiceDto.Description,
            Price = request.ServiceDto.Price,
            DurationMinutes = request.ServiceDto.DurationMinutes,
            //ServiceType = Enum.Parse<ServiceType>(request.ServiceDto.ServiceType)

        };

        await _serviceRepository.AddAsync(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return service.Id;
    }
}