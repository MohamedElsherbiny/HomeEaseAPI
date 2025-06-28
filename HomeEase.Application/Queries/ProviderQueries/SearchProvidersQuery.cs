using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.ProviderQueries;
using HomeEase.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.ProviderQueries
{
    public class SearchProvidersQuery : IRequest<List<ProviderSearchResultDto>>
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? MaxDistance { get; set; }
        public decimal? MinRating { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

// Query Handler
public class SearchProvidersQueryHandler : IRequestHandler<SearchProvidersQuery, List<ProviderSearchResultDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;

    public SearchProvidersQueryHandler(IProviderRepository providerRepository, IMapper mapper)
    {
        _providerRepository = providerRepository;
        _mapper = mapper;
    }

    public async Task<List<ProviderSearchResultDto>> Handle(SearchProvidersQuery request, CancellationToken cancellationToken)
    {
        var providers = await _providerRepository.SearchProvidersAsync(
            request.Latitude,
            request.Longitude,
            request.MaxDistance,
            request.MinRating,
            request.City,
            request.State,
            request.PageNumber,
            request.PageSize);

        return _mapper.Map<List<ProviderSearchResultDto>>(providers);
    }
}