using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Application.Queries.UserQueries;
using Massage.Domain.Common;
using MediatR;

namespace Massage.Application.Queries.UserQueries
{
    public class GetAllUsersQuery : IRequest<PaginatedList<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;

        public bool? IsActive { get; set; } 
    }
}


// Query Handler
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedList<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            request.IsActive);
        var usersToReturn =  _mapper.Map<List<UserDto>>(users);

        return new PaginatedList<UserDto>(usersToReturn, totalCount, request.Page, request.PageSize);
    }
}
