using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Exceptions;
using MediatR;

namespace Massage.Application.Commands.UserCommends;

public class UpdateUserCommand(Guid userId, UpdateUserDto dto) : IRequest<UserDto>
{
    public Guid UserId { get; set; } = userId;
    public string FirstName { get; set; } = dto.FirstName;
    public string LastName { get; set; } = dto.LastName;
    public string PhoneNumber { get; set; } = dto.PhoneNumber;
    public string ProfileImageUrl { get; set; } = dto.ProfileImageUrl;
}

public class UpdateUserCommandHandler(IUserRepository _userRepository, IMapper _mapper, IUnitOfWork _unitOfWork) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.ProfileImageUrl = request.ProfileImageUrl ?? user.ProfileImageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}