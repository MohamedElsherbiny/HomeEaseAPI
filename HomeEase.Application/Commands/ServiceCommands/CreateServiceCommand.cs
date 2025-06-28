using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands
{
    public class CreateServiceCommand : IRequest<Guid>
    {
        public Guid ProviderId { get; set; }
        public CreateServiceDto ServiceDto { get; set; }
    }

    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IBasePlatformServiceRepository _basePlatformServiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateServiceCommandHandler(
            IProviderRepository providerRepository,
            IServiceRepository serviceRepository,
            IBasePlatformServiceRepository basePlatformServiceRepository,
            IUnitOfWork unitOfWork)
        {
            _providerRepository = providerRepository;
            _serviceRepository = serviceRepository;
            _basePlatformServiceRepository = basePlatformServiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            // Validate provider exists
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
            if (provider == null)
                throw new ApplicationException($"Provider with ID {request.ProviderId} not found");

            // Validate BasePlatformService exists and is active
            var basePlatformService = await _basePlatformServiceRepository.GetByIdAsync(request.ServiceDto.BasePlatformServiceId);
            if (basePlatformService == null || !basePlatformService.IsActive)
                throw new ApplicationException($"BasePlatformService with ID {request.ServiceDto.BasePlatformServiceId} not found or inactive");

            var service = new Service
            {
                Id = Guid.NewGuid(),
                ProviderId = request.ProviderId,
                BasePlatformServiceId = request.ServiceDto.BasePlatformServiceId,
                Name = request.ServiceDto.Name,
                NameAr = request.ServiceDto.NameAr,
                Description = request.ServiceDto.Description,
                DescriptionAr = request.ServiceDto.DescriptionAr,
                Price = request.ServiceDto.Price,
                HomePrice = request.ServiceDto.HomePrice,
                DurationMinutes = request.ServiceDto.DurationMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _serviceRepository.AddAsync(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return service.Id;
        }
    }
}