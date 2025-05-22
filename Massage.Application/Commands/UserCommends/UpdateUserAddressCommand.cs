using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Exceptions;
using MediatR;

namespace Massage.Application.Commands.UserCommends;

public class UpdateUserAddressCommand(Guid userId, Guid addressId, AddressDto address) : IRequest<AddressDto>
{
    public Guid UserId { get; set; } = userId;
    public Guid AddressId { get; set; } = addressId;
    public AddressDto Address { get; set; } = address;
}

public class UpdateUserAddressCommandHandler(
    IAddressRepository _addressRepository,
    IMapper _mapper,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateUserAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.AddressId);
        if (address == null || address.UserId != request.UserId)
        {
            throw new BusinessException($"Address not found or does not belong to user {request.UserId}.");
        }

        address.Street = request.Address.Street ?? address.Street;
        address.City = request.Address.City ?? address.City;
        address.State = request.Address.State ?? address.State;
        address.PostalCode = request.Address.PostalCode ?? address.PostalCode;
        address.Country = request.Address.Country ?? address.Country;
        address.Latitude = request.Address.Latitude ?? address.Latitude;
        address.Longitude = request.Address.Longitude ?? address.Longitude;
        address.UpdatedAt = DateTime.UtcNow;

        _addressRepository.Update(address);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}