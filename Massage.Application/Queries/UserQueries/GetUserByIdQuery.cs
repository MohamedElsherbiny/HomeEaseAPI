using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Exceptions;
using MediatR;

namespace Massage.Application.Queries.UserQueries;

public class GetUserByIdQuery(Guid userId) : IRequest<UserDto>
{
    public Guid UserId { get; set; } = userId;
}

public class GetUserByIdQueryHandler(IUserRepository _userRepository, IMapper _mapper) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        return _mapper.Map<UserDto>(user);
    }
}