using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Exceptions;
using MediatR;

namespace HomeEase.Application.Queries.UserQueries;

public class GetUserPreferencesQuery(Guid userId) : IRequest<UserPreferencesDto>
{
    public Guid UserId { get; set; } = userId;
}

public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, UserPreferencesDto>
{
    private readonly IUserPreferencesRepository _preferencesRepository;
    private readonly IMapper _mapper;

    public GetUserPreferencesQueryHandler(IUserPreferencesRepository preferencesRepository, IMapper mapper)
    {
        _preferencesRepository = preferencesRepository;
        _mapper = mapper;
    }

    public async Task<UserPreferencesDto> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await _preferencesRepository.GetByUserIdAsync(request.UserId);
        if (preferences == null)
        {
            throw new BusinessException($"Preferences for user {request.UserId} not found.");
        }

        return _mapper.Map<UserPreferencesDto>(preferences);
    }
}