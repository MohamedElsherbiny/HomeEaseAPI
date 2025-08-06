using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;
public class DeleteUserAddressCommand(Guid userId, Guid addressId) : IRequest<EntityResult>
{
    public Guid UserId { get; set; } = userId;
    public Guid AddressId { get; set; } = addressId;
}

public class DeleteUserAddressCommandHandler(
    IAddressRepository _addressRepository,
    IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserAddressCommand, EntityResult>
{
    public async Task<EntityResult> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.AddressId);
        if (address == null || address.UserId != request.UserId)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.AddressNotFoundOrUnauthorized), Messages.AddressNotFoundOrUnauthorized));
        }

        _addressRepository.Delete(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}