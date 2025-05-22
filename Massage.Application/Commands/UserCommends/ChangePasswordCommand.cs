using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Exceptions;
using MediatR;

namespace Massage.Application.Commands.UserCommends;

public class ChangePasswordCommand(Guid userId, ChangePasswordDto dto) : IRequest<bool>
{
    public Guid UserId { get; set; } = userId;
    public string CurrentPassword { get; set; } = dto.CurrentPassword;
    public string NewPassword { get; set; } = dto.NewPassword;
}

public class ChangePasswordCommandHandler(IUserRepository _userRepository, IPasswordHasher _passwordHasher, IUnitOfWork _unitOfWork) : IRequestHandler<ChangePasswordCommand, bool>
{
    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            throw new BusinessException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}