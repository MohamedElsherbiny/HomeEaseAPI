using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
using Massage.Application.Queries.UserQueries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.UserQueries
{
    public class GetUserPreferencesQuery : IRequest<UserPreferencesDto>
    {
        public Guid UserId { get; set; }

        public GetUserPreferencesQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}


// Query Handler
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
            throw new NotFoundException($"Preferences for user {request.UserId} not found.");

        return _mapper.Map<UserPreferencesDto>(preferences);
    }
}