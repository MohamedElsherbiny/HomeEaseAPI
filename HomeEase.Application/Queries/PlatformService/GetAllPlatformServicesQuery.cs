using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Queries.PlatformService
{
    public class GetAllPlatformServicesQuery : IRequest<PaginatedList<BasePlatformServiceDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public EnumExportFormat ExportFormat { get; set; } = EnumExportFormat.Excel;
    }

    public class GetAllPlatformServicesHandler(IAppDbContext _context, IMapper _mapper) : IRequestHandler<GetAllPlatformServicesQuery, PaginatedList<BasePlatformServiceDto>>
    {
        public async Task<PaginatedList<BasePlatformServiceDto>> Handle(GetAllPlatformServicesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BasePlatformService.AsQueryable().Where(s => s.IsActive);

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(s => s.Name.Contains(request.SearchTerm));
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                _ => request.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            };

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