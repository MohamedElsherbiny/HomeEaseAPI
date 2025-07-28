using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands
{
    public class CreateServicesCommand : IRequest<List<Guid>>
    {
        public Guid ProviderId { get; set; }
        public CreateServicesDto ServicesDto { get; set; }
    }

    public class CreateServicesCommandHandler(
        IProviderRepository _providerRepository,
        IServiceRepository _serviceRepository,
        IBasePlatformServiceRepository _basePlatformServiceRepository,
        IUnitOfWork _unitOfWork) : IRequestHandler<CreateServicesCommand, List<Guid>>
    {
        public async Task<List<Guid>> Handle(CreateServicesCommand request, CancellationToken cancellationToken)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId)
                ?? throw new ApplicationException($"Provider with ID {request.ProviderId} not found");

            if (request.ServicesDto == null || !request.ServicesDto.Services.Any())
            {
                return [];
            }

            var createdOrUpdatedServiceIds = new List<Guid>();

            foreach (var serviceDto in request.ServicesDto.Services)
            {
                var basePlatformService = await _basePlatformServiceRepository.GetByIdAsync(serviceDto.BasePlatformServiceId);
                if (basePlatformService == null || !basePlatformService.IsActive)
                    throw new ApplicationException($"BasePlatformService with ID {serviceDto.BasePlatformServiceId} not found or inactive for one of the services");

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

            return createdOrUpdatedServiceIds;
        }
    }
}