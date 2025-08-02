using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands
{
    public class CreateServiceCommand : IRequest<EntityResult>
    {
        public Guid ProviderId { get; set; }
        public CreateServiceDto ServiceDto { get; set; }
    }

    public class CreateServiceCommandHandler(
        IProviderRepository providerRepository,
        IServiceRepository serviceRepository,
        IBasePlatformServiceRepository basePlatformServiceRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateServiceCommand, EntityResult>
    {
        public async Task<EntityResult> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            var provider = await providerRepository.GetByIdAsync(request.ProviderId);
            if (provider is null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.ProviderNotFound), string.Format(Messages.ProviderNotFound, request.ProviderId)));
            }

            var basePlatformService = await basePlatformServiceRepository.GetByIdAsync(request.ServiceDto.BasePlatformServiceId);
            if (basePlatformService == null || !basePlatformService.IsActive)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.BasePlatformServiceNotFound), string.Format(Messages.BasePlatformServiceNotFound, request.ServiceDto.BasePlatformServiceId)));
            }

            var existingService = await serviceRepository.FindAsync(s =>
                s.ProviderId == request.ProviderId &&
                s.BasePlatformServiceId == request.ServiceDto.BasePlatformServiceId);

            if (existingService != null)
            {
                return EntityResult.SuccessWithData(new { serviceId = existingService.Id });
            }

            var service = new Service
            {
                Id = Guid.NewGuid(),
                ProviderId = request.ProviderId,
                BasePlatformServiceId = request.ServiceDto.BasePlatformServiceId,
                Name = basePlatformService.Name,
                NameAr = basePlatformService.NameAr,
                Description = basePlatformService.Description,
                DescriptionAr = basePlatformService.DescriptionAr,
                Price = request.ServiceDto.Price,
                HomePrice = request.ServiceDto.HomePrice,
                DurationMinutes = 60,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await serviceRepository.AddAsync(service);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return EntityResult.SuccessWithData(new { serviceId = service.Id });
        }
    }
}