using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Interfaces;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.AdminQueries
{
    // Query to get users with filters for admin
    public class GetFilteredUsersQuery : IRequest<List<UserDto>>
    {
        public string? SearchTerm { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetFilteredUsersQueryHandler : IRequestHandler<GetFilteredUsersQuery, List<UserDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetFilteredUsersQueryHandler(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.Email.Contains(request.SearchTerm) ||
                    u.FirstName.Contains(request.SearchTerm) ||
                    u.LastName.Contains(request.SearchTerm) ||
                    u.PhoneNumber.Contains(request.SearchTerm));
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.Role == request.Role.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= request.ToDate.Value);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
