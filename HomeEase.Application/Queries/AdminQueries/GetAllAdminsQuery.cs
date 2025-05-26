using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using MediatR;

namespace HomeEase.Application.Queries.AdminQueries
{
    // Query to get all Admins with pagination
    public class GetAllAdminsQuery : IRequest<PaginatedList<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public bool? IsActive { get; set; }
        public EnumExportFormat ExportFormat { get; set; } = EnumExportFormat.Excel;
    }

    public class GetAllAdminsQueryHandler : IRequestHandler<GetAllAdminsQuery, PaginatedList<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAllAdminsQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserDto>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
        {
            var (users, totalCount) = await _userRepository.GetAllAsync(
                request.Page,
                request.PageSize,
                request.SearchTerm,
                request.SortBy,
                request.SortDescending,
                request.IsActive);

            var adminUsers = users.Where(u => u.Role == UserRole.Admin).ToList();
            var adminUsersDto = _mapper.Map<List<UserDto>>(adminUsers);

            return new PaginatedList<UserDto>(adminUsersDto, totalCount, request.Page, request.PageSize);
        }
    }
}