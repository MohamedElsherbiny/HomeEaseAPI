using Massage.Application.Interfaces.Services;
using Massage.Application.Interfaces;
using MediatR;
using Massage.Domain.Exceptions;

namespace Massage.Application.Commands.UserCommends;
public class DeleteUserAddressCommand(Guid userId, Guid addressId) : IRequest<bool>
{
    public Guid UserId { get; set; } = userId;
    public Guid AddressId { get; set; } = addressId;
}

public class DeleteUserAddressCommandHandler(
    IAddressRepository _addressRepository,
    IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserAddressCommand, bool>
{
    public async Task<bool> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.AddressId);
        if (address == null || address.UserId != request.UserId)
            throw new BusinessException($"Address not found or does not belong to user {request.UserId}.");

        _addressRepository.Delete(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}