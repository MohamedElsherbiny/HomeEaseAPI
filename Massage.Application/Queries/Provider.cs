using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Massage.Application.DTOs;
using Massage.Domain.Repositories;
using AutoMapper;

namespace Massage.Application.Queries
{
    // Provider Queries
    public class GetProviderByIdQuery : IRequest<ProviderDto>
    {
        public Guid ProviderId { get; set; }
    }

    public class GetProviderByUserIdQuery : IRequest<ProviderDto>
    {
        public Guid UserId { get; set; }
    }

    public class GetAllProvidersQuery : IRequest<List<ProviderDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SearchProvidersQuery : IRequest<List<ProviderSearchResultDto>>
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? MaxDistance { get; set; }
        public string[] ServiceTypes { get; set; }
        public decimal? MinRating { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // Service Queries
    public class GetServiceByIdQuery : IRequest<ServiceDto>
    {
        public Guid ServiceId { get; set; }
    }

    public class GetServicesByProviderQuery : IRequest<List<ServiceDto>>
    {
        public Guid ProviderId { get; set; }
    }

    // Query Handlers
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
                request.ServiceTypes,
                request.MinRating,
                request.City,
                request.State,
                request.PageNumber,
                request.PageSize);

            return _mapper.Map<List<ProviderSearchResultDto>>(providers);
        }
    }

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
}

