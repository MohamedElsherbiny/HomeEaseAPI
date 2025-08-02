using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands;

public class UpdateServiceCommand : IRequest<EntityResult>
{
    public Guid ServiceId { get; set; }
    public UpdateServiceDto ServiceDto { get; set; }
}

public class UpdateServiceCommandHandler(
    IServiceRepository _serviceRepository,
    IBasePlatformServiceRepository _basePlatformServiceRepository,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateServiceCommand, EntityResult>
{
    public async Task<EntityResult> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
        if (service is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ServiceNotFound), Messages.ServiceNotFound));
        }

        var basePlatformService = await _basePlatformServiceRepository.GetByIdAsync(request.ServiceDto.BasePlatformServiceId);
        if (basePlatformService == null || !basePlatformService.IsActive)
        {
            return EntityResult.Failed(new EntityError(
                nameof(Messages.BasePlatformServiceNotFoundOrInactive),
                string.Format(Messages.BasePlatformServiceNotFoundOrInactive, request.ServiceDto.BasePlatformServiceId)));
        }

        service.BasePlatformServiceId = request.ServiceDto.BasePlatformServiceId;

        if (request.ServiceDto.Price.HasValue)
            service.Price = request.ServiceDto.Price.Value;

        if (request.ServiceDto.HomePrice.HasValue)
            service.Price = request.ServiceDto.HomePrice.Value;

        service.UpdatedAt = DateTime.UtcNow;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}