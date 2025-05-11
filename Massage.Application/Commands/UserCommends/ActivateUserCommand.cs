using Massage.Application.Commands.UserCommends;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.UserCommends
{
    public class ActivateUserCommand : IRequest<bool>
    {
        public Guid UserId { get; }

        public ActivateUserCommand(Guid userId)
        {
            UserId = userId;
        }
    }

}


// Command Handler
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeactivatedAt = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
