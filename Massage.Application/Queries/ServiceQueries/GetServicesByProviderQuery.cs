using AutoMapper;
using Massage.Application.DTOs;
using Massage.Application.Queries;
using Massage.Application.Queries.ServiceQueries;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.ServiceQueries
{
    public class GetServicesByProviderQuery : IRequest<List<ServiceDto>>
    {
        public Guid ProviderId { get; set; }
    }
}

// Query Handler
public class GetServicesByProviderQueryHandler : IRequestHandler<GetServicesByProviderQuery, List<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMapper _mapper;

    public GetServicesByProviderQueryHandler(IServiceRepository serviceRepository, IMapper mapper)
    {
        _serviceRepository = serviceRepository;
        _mapper = mapper;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesByProviderQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetByProviderIdAsync(request.ProviderId);
        return _mapper.Map<List<ServiceDto>>(services);
    }
}
