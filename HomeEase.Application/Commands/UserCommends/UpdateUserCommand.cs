using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.UserCommends;

public class UpdateUserCommand(Guid userId, UpdateUserDto dto) : IRequest<EntityResult>
{
    public Guid UserId { get; set; } = userId;
    public string? FirstName { get; set; } = dto.FirstName;
    public string? LastName { get; set; } = dto.LastName;
    public string? PhoneNumber { get; set; } = dto.PhoneNumber;
    public string? ProfileImageUrl { get; set; } = dto.ProfileImageUrl;
}

public class UpdateUserCommandHandler(IUserRepository _userRepository, IMapper _mapper, IUnitOfWork _unitOfWork) : IRequestHandler<UpdateUserCommand, EntityResult>
{
    public async Task<EntityResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFound), string.Format(Messages.UserNotFound, request.UserId)));
        }

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.ProfileImageUrl = request.ProfileImageUrl ?? user.ProfileImageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.SuccessWithData(new { user = _mapper.Map<UserDto>(user) });
    }
}