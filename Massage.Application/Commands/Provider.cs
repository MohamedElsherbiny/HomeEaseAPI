using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Massage.Application.DTOs;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Massage.Domain.Enums;
using Massage.Application.Interfaces.Services;

namespace Massage.Application.Commands
{
    // Provider Commands
    public class CreateProviderCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string[] ServiceTypes { get; set; }
        public AddressDto Address { get; set; }
    }

    public class UpdateProviderCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
        public UpdateProviderDto ProviderDto { get; set; }
    }

    public class DeleteProviderCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
    }

    public class VerifyProviderCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
    }

    public class UpdateProviderScheduleCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
        public ProviderScheduleDto ScheduleDto { get; set; }
    }

    // Service Commands
    public class CreateServiceCommand : IRequest<Guid>
    {
        public Guid ProviderId { get; set; }
        public CreateServiceDto ServiceDto { get; set; }
    }

    public class UpdateServiceCommand : IRequest<bool>
    {
        public Guid ServiceId { get; set; }
        public UpdateServiceDto ServiceDto { get; set; }
    }

    public class DeleteServiceCommand : IRequest<bool>
    {
        public Guid ServiceId { get; set; }
    }

    // Command Handlers
    public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Guid>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
        {
            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BusinessName = request.BusinessName,
                Description = request.Description,
                ProfileImageUrl = request.ProfileImageUrl,
                ServiceTypes = request.ServiceTypes.ToList(),
                Address = new Address
                {
                    Street = request.Address.Street,
                    City = request.Address.City,
                    State = request.Address.State,
                    PostalCode = request.Address.PostalCode,
                    Country = request.Address.Country,
                    Latitude = request.Address.Latitude,
                    Longitude = request.Address.Longitude
                },
                Rating = 0,
                ReviewCount = 0,
                Status = Enum.Parse<ProviderStatus>("Pending"),
                CreatedAt = DateTime.UtcNow,
                VerifiedAt = null,
                Schedule = new ProviderSchedule
                {
                    RegularHours = new List<WorkingHours>(),
                    SpecialDates = new List<SpecialDate>(),
                    AvailableSlots = new List<TimeSlot>()
                },
                Services = new List<Service>()
            };

            await _providerRepository.AddAsync(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return provider.Id;
        }
    }

    public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, bool>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
            if (provider == null)
                return false;

            provider.BusinessName = request.ProviderDto.BusinessName ?? provider.BusinessName;
            provider.Description = request.ProviderDto.Description ?? provider.Description;
            provider.ProfileImageUrl = request.ProviderDto.ProfileImageUrl ?? provider.ProfileImageUrl;
            provider.ServiceTypes = request.ProviderDto.ServiceTypes?.ToList() ?? provider.ServiceTypes;

            if (request.ProviderDto.Address != null)
            {
                provider.Address.Street = request.ProviderDto.Address.Street ?? provider.Address.Street;
                provider.Address.City = request.ProviderDto.Address.City ?? provider.Address.City;
                provider.Address.State = request.ProviderDto.Address.State ?? provider.Address.State;
                provider.Address.PostalCode = request.ProviderDto.Address.PostalCode ?? provider.Address.PostalCode;
                provider.Address.Country = request.ProviderDto.Address.Country ?? provider.Address.Country;
                provider.Address.Latitude = request.ProviderDto.Address.Latitude ?? provider.Address.Latitude;
                provider.Address.Longitude = request.ProviderDto.Address.Longitude ?? provider.Address.Longitude;
            }

            _providerRepository.Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, bool>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
            if (provider == null)
                return false;

            _providerRepository.Delete(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class VerifyProviderCommandHandler : IRequestHandler<VerifyProviderCommand, bool>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(VerifyProviderCommand request, CancellationToken cancellationToken)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
            if (provider == null)
                return false;

            provider.Status = ProviderStatus.Suspended;
            provider.VerifiedAt = DateTime.UtcNow;

            _providerRepository.Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class UpdateProviderScheduleCommandHandler : IRequestHandler<UpdateProviderScheduleCommand, bool>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProviderScheduleCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateProviderScheduleCommand request, CancellationToken cancellationToken)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
            if (provider == null)
                return false;

            provider.Schedule.RegularHours = request.ScheduleDto.RegularHours.Select(wh => new WorkingHours
            {
                DayOfWeek = (DayOfWeek)wh.DayOfWeek,
                StartTime = wh.StartTime,
                EndTime = wh.EndTime,
                IsOpen = wh.IsOpen
            }).ToList();

            provider.Schedule.SpecialDates = request.ScheduleDto.SpecialDates.Select(sd => new SpecialDate
            {
                Date = sd.Date,
                StartTime = sd.StartTime,
                EndTime = sd.EndTime,
                IsClosed = sd.IsClosed,
                Note = sd.Note
            }).ToList();

            provider.Schedule.AvailableSlots = request.ScheduleDto.AvailableSlots.Select(ts => new TimeSlot
            {
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                IsAvailable = ts.IsAvailable
            }).ToList();

            _providerRepository.Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
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
                ServiceType = Enum.Parse<ServiceType>(request.ServiceDto.ServiceType)

            };

            await _serviceRepository.AddAsync(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return service.Id;
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

            if (request.ServiceDto.ServiceType != null)
                service.ServiceType = Enum.Parse<ServiceType>(request.ServiceDto.ServiceType);


            _serviceRepository.Update(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
        {
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
                return false;

            _serviceRepository.Delete(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}