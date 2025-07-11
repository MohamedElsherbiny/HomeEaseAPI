using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands;

public class UpdateServiceCommand : IRequest<bool>
{
    public Guid ServiceId { get; set; }
    public UpdateServiceDto ServiceDto { get; set; }
}

public class UpdateServiceCommandHandler(
    IServiceRepository _serviceRepository,
    IBasePlatformServiceRepository _basePlatformServiceRepository,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateServiceCommand, bool>
{
    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
        if (service == null)
            return false;

        // Validate BasePlatformService exists and is active
        var basePlatformService = await _basePlatformServiceRepository.GetByIdAsync(request.ServiceDto.BasePlatformServiceId);
        if (basePlatformService == null || !basePlatformService.IsActive)
            throw new ApplicationException($"BasePlatformService with ID {request.ServiceDto.BasePlatformServiceId} not found or inactive");

        service.BasePlatformServiceId = request.ServiceDto.BasePlatformServiceId;

        if (request.ServiceDto.Price.HasValue)
            service.Price = request.ServiceDto.Price.Value;

        if (request.ServiceDto.HomePrice.HasValue)
            service.Price = request.ServiceDto.HomePrice.Value;

        service.UpdatedAt = DateTime.UtcNow;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}