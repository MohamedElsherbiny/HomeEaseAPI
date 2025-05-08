using AutoMapper;
using Massage.Application.Commands.UserCommends;
using Massage.Application.DTOs;
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
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }

        public UpdateUserCommand(Guid userId, UpdateUserDto dto)
        {
            UserId = userId;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            PhoneNumber = dto.PhoneNumber;
            ProfileImageUrl = dto.ProfileImageUrl;
        }
    }
}


// Command Handler
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found.");

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.ProfileImageUrl = request.ProfileImageUrl ?? user.ProfileImageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}