using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Massage.Application.Commands.ProviderCommands;

namespace Massage.Application.Commands.ProviderCommands
{
    public class UpdateProviderCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
        public UpdateProviderDto ProviderDto { get; set; }
    }
}


// Command Handler
public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, bool>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAddressRepository _addressRepository;

    public UpdateProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork, IAddressRepository addressRepository)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _addressRepository = addressRepository;
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
            if (provider.Address == null)
            {
                provider.Address = new Address
                {
                    UserId = provider.UserId,
                    Street = request.ProviderDto.Address.Street,
                    City = request.ProviderDto.Address.City,
                    State = request.ProviderDto.Address.State,
                    PostalCode = request.ProviderDto.Address.PostalCode,
                    Country = request.ProviderDto.Address.Country,
                    Latitude = request.ProviderDto.Address.Latitude,
                    Longitude = request.ProviderDto.Address.Longitude,
                    ZipCode = request.ProviderDto.Address.ZipCode,
                    CreatedAt = DateTime.UtcNow
                };

            }
            else
            {
                provider.Address.Street = request.ProviderDto.Address.Street ?? provider.Address.Street;
                provider.Address.City = request.ProviderDto.Address.City ?? provider.Address.City;
                provider.Address.State = request.ProviderDto.Address.State ?? provider.Address.State;
                provider.Address.PostalCode = request.ProviderDto.Address.PostalCode ?? provider.Address.PostalCode;
                provider.Address.Country = request.ProviderDto.Address.Country ?? provider.Address.Country;
                provider.Address.Latitude = request.ProviderDto.Address.Latitude ?? provider.Address.Latitude;
                provider.Address.Longitude = request.ProviderDto.Address.Longitude ?? provider.Address.Longitude;
                provider.Address.ZipCode = request.ProviderDto.Address.ZipCode ?? provider.Address.ZipCode;
                provider.Address.UpdatedAt = DateTime.UtcNow;
            }
        }

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}