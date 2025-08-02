using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;

public class ChangePasswordCommand(Guid userId, ChangePasswordDto dto) : IRequest<EntityResult>
{
    public Guid UserId { get; set; } = userId;
    public string CurrentPassword { get; set; } = dto.CurrentPassword;
    public string NewPassword { get; set; } = dto.NewPassword;
}

public class ChangePasswordCommandHandler(IUserRepository _userRepository, IPasswordHasher _passwordHasher, IUnitOfWork _unitOfWork) : IRequestHandler<ChangePasswordCommand, EntityResult>
{
    public async Task<EntityResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFound), string.Format(Messages.UserNotFound, request.UserId)));
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.IncorrectCurrentPassword), Messages.IncorrectCurrentPassword));
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}