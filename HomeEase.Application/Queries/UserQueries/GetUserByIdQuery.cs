using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Exceptions;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Queries.UserQueries;

public class GetUserByIdQuery(Guid userId) : IRequest<UserDto>
{
    public Guid UserId { get; set; } = userId;
}

public class GetUserByIdQueryHandler(IUserRepository _userRepository, IProviderRepository _providerRepository, IMapper _mapper) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException($"User with ID {request.UserId} not found.");
        }

        var userToReturn = _mapper.Map<UserDto>(user);
        if (user.Role == Domain.Enums.UserRole.Provider)
        {
            userToReturn.ProviderId = (await _providerRepository.GetByUserIdAsync(user.Id)).Id.ToString();
        }

        return userToReturn;
    }
}