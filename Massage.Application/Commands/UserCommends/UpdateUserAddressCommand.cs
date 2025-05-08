using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;
using Massage.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Massage.Application.Commands.UserCommends;

namespace Massage.Application.Commands.UserCommends
{
    public class UpdateUserAddressCommand : IRequest<AddressDto>
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
        public AddressDto Address { get; set; }

        public UpdateUserAddressCommand(Guid userId, Guid addressId, AddressDto address)
        {
            UserId = userId;
            AddressId = addressId;
            Address = address;
        }
    }
}


// Command Handler
public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, AddressDto>
{
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserAddressCommandHandler(
        IAddressRepository addressRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddressDto> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.AddressId);
        if (address == null || address.UserId != request.UserId)
            throw new NotFoundException($"Address not found or does not belong to user {request.UserId}.");

        address.Street = request.Address.Street ?? address.Street;
        address.City = request.Address.City ?? address.City;
        address.State = request.Address.State ?? address.State;
        address.PostalCode = request.Address.PostalCode ?? address.PostalCode;
        address.Country = request.Address.Country ?? address.Country;
        address.Latitude = request.Address.Latitude ?? address.Latitude;
        address.Longitude = request.Address.Longitude ?? address.Longitude;
        address.UpdatedAt = DateTime.UtcNow;

        _addressRepository.Update(address);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }
}