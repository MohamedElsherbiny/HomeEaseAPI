using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Queries.ProviderQueries;
using Massage.Domain.Common;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.ProviderQueries
{
    public class GetAllProvidersQuery : IRequest<PaginatedList<ProviderDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;

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
            request.SortDescending);

        var mappedProviders = _mapper.Map<List<ProviderDto>>(providers);

        return new PaginatedList<ProviderDto>(mappedProviders, totalCount, request.PageNumber, request.PageSize);
    }
}
