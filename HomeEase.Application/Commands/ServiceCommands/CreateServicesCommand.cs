using HomeEase.Application.DTOs.Common;
using HomeEase.Application.DTOs.ProviderService;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands
{
    public class CreateServicesCommand : IRequest<EntityResult>
    {
        public Guid ProviderId { get; set; }
        public BlukUpdateServicesDto ServicesDto { get; set; }
    }

    public class CreateServicesCommandHandler(
        IProviderRepository _providerRepository,
        IServiceRepository _serviceRepository,
        IBasePlatformServiceRepository _basePlatformServiceRepository,
        IUnitOfWork _unitOfWork) : IRequestHandler<CreateServicesCommand, EntityResult>
    {
        public async Task<EntityResult> Handle(CreateServicesCommand request, CancellationToken cancellationToken)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
            if (provider == null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.ProviderNotFound), string.Format(Messages.ProviderNotFound, request.ProviderId)));
            }

            if (request.ServicesDto == null || request.ServicesDto.Services.Count == 0)
            {
                return EntityResult.Success;
            }

            var createdOrUpdatedServiceIds = new List<Guid>();

            foreach (var serviceDto in request.ServicesDto.Services)
            {
                var basePlatformService = await _basePlatformServiceRepository.GetByIdAsync(serviceDto.BasePlatformServiceId);
                if (basePlatformService == null || !basePlatformService.IsActive)
                {
                    return EntityResult.Failed(new EntityError(nameof(Messages.BasePlatformServiceNotFoundForService),
                        string.Format(Messages.BasePlatformServiceNotFoundForService, serviceDto.BasePlatformServiceId)));
                }

                var existingService = await _serviceRepository.FindAsync(s =>
                 s.ProviderId == request.ProviderId &&
                 s.BasePlatformServiceId == serviceDto.BasePlatformServiceId);

                if (existingService != null)
                {
                    existingService.Price = serviceDto.Price;
                    existingService.HomePrice = serviceDto.HomePrice;
                    _serviceRepository.Update(existingService);
                    createdOrUpdatedServiceIds.Add(existingService.Id);
                }
                else
                {
                    var service = new Service
                    {
                        Id = Guid.NewGuid(),
                        ProviderId = request.ProviderId,
                        BasePlatformServiceId = serviceDto.BasePlatformServiceId,
                        Name = basePlatformService.Name,
                        NameAr = basePlatformService.NameAr,
                        Description = basePlatformService.Description,
                        DescriptionAr = basePlatformService.DescriptionAr,
                        Price = serviceDto.Price,
                        HomePrice = serviceDto.HomePrice,
                        DurationMinutes = 60,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _serviceRepository.AddAsync(service);
                    createdOrUpdatedServiceIds.Add(service.Id);
                }


            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EntityResult.SuccessWithData(new { createdOrUpdatedServiceIds });
        }
    }
}