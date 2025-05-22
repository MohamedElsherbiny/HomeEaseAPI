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
    public class GetProviderByIdQuery : IRequest<ProviderDto>
    {
        public Guid ProviderId { get; set; }
    }
}


// Query Handler
public class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, ProviderDto>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;

    public GetProviderByIdQueryHandler(IProviderRepository providerRepository, IMapper mapper)
    {
        _providerRepository = providerRepository;
        _mapper = mapper;
    }

    public async Task<ProviderDto> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdWithDetailsAsync(request.ProviderId);
        return _mapper.Map<ProviderDto>(provider);
    }
}