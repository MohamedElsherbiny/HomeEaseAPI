using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;
using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Massage.Application.Commands.UserCommends;

namespace Massage.Application.Commands.UserCommends
{
    public class AddUserAddressCommand : IRequest<AddressDto>
    {
        public Guid UserId { get; set; }
        public AddressDto Address { get; set; }

        public AddUserAddressCommand(Guid userId, AddressDto address)
        {
            UserId = userId;
            Address = address;
        }
    }
}


// Command Handler
public class AddUserAddressCommandHandler : IRequestHandler<AddUserAddressCommand, AddressDto>
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public AddUserAddressCommandHandler(
        IAddressRepository addressRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddressDto> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        var address = _mapper.Map<Address>(request.Address);
        address.UserId = request.UserId;
        address.Id = Guid.NewGuid();
        address.CreatedAt = DateTime.UtcNow;

        await _addressRepository.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }
}