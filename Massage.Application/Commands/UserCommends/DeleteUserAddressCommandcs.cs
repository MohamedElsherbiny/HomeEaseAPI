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
    public class DeleteUserAddressCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }

        public DeleteUserAddressCommand(Guid userId, Guid addressId)
        {
            UserId = userId;
            AddressId = addressId;
        }
    }
}


// Command Handler
public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, bool>
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserAddressCommandHandler(
        IAddressRepository addressRepository,
        IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.AddressId);
        if (address == null || address.UserId != request.UserId)
            throw new NotFoundException($"Address not found or does not belong to user {request.UserId}.");

        _addressRepository.Delete(address);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}