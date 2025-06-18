using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.UserQueries;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using MediatR;

namespace HomeEase.Application.Queries.UserQueries
{
    public class GetAllUsersQuery : IRequest<PaginatedList<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public EnumExportFormat ExportFormat { get; set; } = EnumExportFormat.Excel;

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
        var (users, _) = await _userRepository.GetAllAsync(
            1,
            int.MaxValue,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            request.IsActive);

        var userRoleUsers = users.Where(u => u.Role == UserRole.User).ToList();

        var totalCount = userRoleUsers.Count;

        var paginatedUsers = userRoleUsers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var userDtos = _mapper.Map<List<UserDto>>(paginatedUsers);

        return new PaginatedList<UserDto>(userDtos, totalCount, request.Page, request.PageSize);
    }


}
