using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.ServiceCommands
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