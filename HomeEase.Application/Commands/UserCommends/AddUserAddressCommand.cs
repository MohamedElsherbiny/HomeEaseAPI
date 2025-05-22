using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;
using HomeEase.Domain.Exceptions;

namespace HomeEase.Application.Commands.UserCommends;

public class AddUserAddressCommand(Guid userId, AddressDto address) : IRequest<AddressDto>
{
    public Guid UserId { get; set; } = userId;
    public AddressDto Address { get; set; } = address;
}

public class AddUserAddressCommandHandler(
    IAddressRepository _addressRepository,
    IUserRepository _userRepository,
    IMapper _mapper,
    IUnitOfWork _unitOfWork) : IRequestHandler<AddUserAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        var address = _mapper.Map<Address>(request.Address);
        address.UserId = request.UserId;
        address.Id = Guid.NewGuid();
        address.CreatedAt = DateTime.UtcNow;

        await _addressRepository.AddAsync(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}