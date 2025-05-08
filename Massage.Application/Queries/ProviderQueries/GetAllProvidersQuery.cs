using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Queries.ProviderQueries;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.ProviderQueries
{
    public class GetAllProvidersQuery : IRequest<List<ProviderDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

// Query Handler
public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, List<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;

    public GetAllProvidersQueryHandler(IProviderRepository providerRepository, IMapper mapper)
    {
        _providerRepository = providerRepository;
        _mapper = mapper;
    }

    public async Task<List<ProviderDto>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
    {
        var providers = await _providerRepository.GetAllWithPaginationAsync(request.PageNumber, request.PageSize);
        return _mapper.Map<List<ProviderDto>>(providers);
    }
}
