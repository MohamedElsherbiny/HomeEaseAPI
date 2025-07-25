﻿using HomeEase.Application.Commands.UserCommends;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Exceptions;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;

public class ActivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; }

    public ActivateUserCommand(Guid userId) => UserId = userId;
}

public class ActivateUserCommandHandler(IUserRepository _userRepository, IUnitOfWork _unitOfWork) : IRequestHandler<ActivateUserCommand, bool>
{
    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeactivatedAt = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
