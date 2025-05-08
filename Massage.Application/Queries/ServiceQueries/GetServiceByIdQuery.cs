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
    public class GetServiceByIdQuery : IRequest<ServiceDto>
    {
        public Guid ServiceId { get; set; }
    }
}

// Query Handler
public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMapper _mapper;

    public GetServiceByIdQueryHandler(IServiceRepository serviceRepository, IMapper mapper)
    {
        _serviceRepository = serviceRepository;
        _mapper = mapper;
    }

    public async Task<ServiceDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
        return _mapper.Map<ServiceDto>(service);
    }
}
