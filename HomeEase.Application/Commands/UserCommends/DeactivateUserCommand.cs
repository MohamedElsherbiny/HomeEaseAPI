using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Exceptions;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;

public class DeactivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; }

    public DeactivateUserCommand(Guid userId)
    {
        UserId = userId;
    }
}

public class DeactivateUserCommandHandler(IUserRepository _userRepository, IUnitOfWork _unitOfWork) : IRequestHandler<DeactivateUserCommand, bool>
{
    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeactivatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}