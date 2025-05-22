using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.PlatformService
{
    public class GetAllPlatformServicesQuery : IRequest<PaginatedList<BasePlatformServiceDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class GetAllPlatformServicesHandler : IRequestHandler<GetAllPlatformServicesQuery, PaginatedList<BasePlatformServiceDto>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAllPlatformServicesHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<BasePlatformServiceDto>> Handle(GetAllPlatformServicesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BasePlatformService.AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(s => s.Name.Contains(request.SearchTerm));
            }

            // Apply sorting
            switch (request.SortBy.ToLower())
            {
                case "name":
                    query = request.SortDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name);
                    break;
                case "createdat":
                    query = request.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt);
                    break;
                default:
                    query = request.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt);
                    break;
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var services = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTO
            var servicesDto = _mapper.Map<List<BasePlatformServiceDto>>(services);

            return new PaginatedList<BasePlatformServiceDto>(servicesDto, totalCount, request.Page, request.PageSize);
        }
    }
}