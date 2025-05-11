using MediatR;
using Massage.Domain.Entities;
using Massage.Application.Commands.UserCommends;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;

namespace Massage.Application.Commands.UserCommends
{
    public class GetUserStatusCommand : IRequest<UserStatusDto>
    {
        public Guid UserId { get; }

        public GetUserStatusCommand(Guid userId)
        {
            UserId = userId;
        }
    }

    public class UserStatusDto
    {
        public bool IsActive { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }
}


// Command Handler
public class GetUserStatusCommandHandler : IRequestHandler<GetUserStatusCommand, UserStatusDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserStatusCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusDto> Handle(GetUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        return new UserStatusDto
        {
            IsActive = user.IsActive,
            DeactivatedAt = user.DeactivatedAt
        };
    }
}
