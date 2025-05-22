using MediatR;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Exceptions;

namespace Massage.Application.Commands.UserCommends;
public class GetUserStatusCommand(Guid userId) : IRequest<UserStatusDto>
{
    public Guid UserId { get; } = userId;
}

public class UserStatusDto
{
    public bool IsActive { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}

public class GetUserStatusCommandHandler(IUserRepository _userRepository) : IRequestHandler<GetUserStatusCommand, UserStatusDto>
{
    public async Task<UserStatusDto> Handle(GetUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        return new UserStatusDto
        {
            IsActive = user.IsActive,
            DeactivatedAt = user.DeactivatedAt
        };
    }
}
