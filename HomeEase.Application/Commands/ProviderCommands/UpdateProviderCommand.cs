using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class UpdateProviderCommand : IRequest<bool>
{
    public Guid ProviderId { get; set; }
    public UpdateProviderDto ProviderDto { get; set; }
}

public class UpdateProviderCommandHandler(IProviderRepository _providerRepository, IUnitOfWork _unitOfWork) : IRequestHandler<UpdateProviderCommand, bool>
{
    public async Task<bool> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
            return false;

        if (!string.IsNullOrWhiteSpace(request.ProviderDto.BusinessName))
        {

            provider.BusinessName = request.ProviderDto.BusinessName;
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderDto.BusinessAddress))
        {

            provider.BusinessAddress = request.ProviderDto.BusinessAddress;
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderDto.PhoneNumber))
        {

            provider.PhoneNumber = request.ProviderDto.PhoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderDto.Email))
        {

            provider.Email = request.ProviderDto.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderDto.Description))
        {

            provider.Description = request.ProviderDto.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderDto.DescriptionAr))
        {

            provider.DescriptionAr = request.ProviderDto.DescriptionAr;
        }

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