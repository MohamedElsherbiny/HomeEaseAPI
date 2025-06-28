using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
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
    public class UpdateServiceCommand : IRequest<bool>
    {
        public Guid ServiceId { get; set; }
        public UpdateServiceDto ServiceDto { get; set; }
    }
}


public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, bool>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IBasePlatformServiceRepository _basePlatformServiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(
        IServiceRepository serviceRepository,
        IBasePlatformServiceRepository basePlatformServiceRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _basePlatformServiceRepository = basePlatformServiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
        if (service == null)
            return false;

        // Validate BasePlatformService exists and is active
        var basePlatformService = await _basePlatformServiceRepository.GetByIdAsync(request.ServiceDto.BasePlatformServiceId);
        if (basePlatformService == null || !basePlatformService.IsActive)
            throw new ApplicationException($"BasePlatformService with ID {request.ServiceDto.BasePlatformServiceId} not found or inactive");

        if (request.ServiceDto.Name != null)
            service.Name = request.ServiceDto.Name;

        if (request.ServiceDto.Description != null)
            service.Description = request.ServiceDto.Description;

        service.BasePlatformServiceId = request.ServiceDto.BasePlatformServiceId;

        if (request.ServiceDto.Price.HasValue)
            service.Price = request.ServiceDto.Price.Value;

        if (request.ServiceDto.HomePrice.HasValue)
            service.Price = request.ServiceDto.HomePrice.Value;

        if (request.ServiceDto.DurationMinutes.HasValue)
            service.DurationMinutes = request.ServiceDto.DurationMinutes.Value;


        service.UpdatedAt = DateTime.UtcNow;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}