using AutoMapper;
using HomeEase.Application.DTOs.Provider;
using HomeEase.Application.Queries.ProviderQueries;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Queries.ProviderQueries
{
    public class GetAllProvidersQuery : IRequest<PaginatedList<ProviderDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public EnumExportFormat ExportFormat { get; set; } = EnumExportFormat.Excel;

      
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public bool? IsHomeServiceAvailable { get; set; }
        public bool? IsCenterServiceAvailable { get; set; }
        public decimal? MinAverageServiceRating { get; set; }
        public List<Guid>? BasePlatformServiceIds { get; set; }
    }

}

// Query Handler
public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, PaginatedList<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;

    public GetAllProvidersQueryHandler(IProviderRepository providerRepository, IMapper mapper)
    {
        _providerRepository = providerRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ProviderDto>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
    {
        var (providers, totalCount) = await _providerRepository.GetAllProvidersAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            request.MinPrice,
            request.MaxPrice,
            request.City,
            request.IsHomeServiceAvailable,
            request.IsCenterServiceAvailable,
            request.MinAverageServiceRating,
            request.BasePlatformServiceIds
        );

        var mappedProviders = _mapper.Map<List<ProviderDto>>(providers);

        return new PaginatedList<ProviderDto>(mappedProviders, totalCount, request.PageNumber, request.PageSize);
    }

}
