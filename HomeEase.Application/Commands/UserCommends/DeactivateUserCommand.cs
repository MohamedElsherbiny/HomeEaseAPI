using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;

public class DeactivateUserCommand(Guid userId) : IRequest<EntityResult>
{
    public Guid UserId { get; } = userId;
}

public class DeactivateUserCommandHandler(IUserRepository _userRepository, IUnitOfWork _unitOfWork) : IRequestHandler<DeactivateUserCommand, EntityResult>
{
    public async Task<EntityResult> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFound), string.Format(Messages.UserNotFound, request.UserId)));
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeactivatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}