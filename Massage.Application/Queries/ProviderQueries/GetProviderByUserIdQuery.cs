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
    public class GetProviderByUserIdQuery : IRequest<ProviderDto>
    {
        public Guid UserId { get; set; }
    }
}

// Query Handler
public class GetProviderByUserIdQueryHandler : IRequestHandler<GetProviderByUserIdQuery, ProviderDto>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;

    public GetProviderByUserIdQueryHandler(IProviderRepository providerRepository, IMapper mapper)
    {
        _providerRepository = providerRepository;
        _mapper = mapper;
    }

    public async Task<ProviderDto> Handle(GetProviderByUserIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByUserIdWithDetailsAsync(request.UserId);
        return _mapper.Map<ProviderDto>(provider);
    }
}
