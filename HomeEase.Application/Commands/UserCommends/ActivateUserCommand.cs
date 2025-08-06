using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;

public class ActivateUserCommand(Guid userId) : IRequest<EntityResult>
{
    public Guid UserId { get; } = userId;
}

public class ActivateUserCommandHandler(IUserRepository _userRepository, IUnitOfWork _unitOfWork) : IRequestHandler<ActivateUserCommand, EntityResult>
{
    public async Task<EntityResult> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFound), string.Format(Messages.UserNotFound, request.UserId)));
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeactivatedAt = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}
