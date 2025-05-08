using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Application.Queries.UserQueries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.UserQueries
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}


// Query Handler
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending);

        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}
