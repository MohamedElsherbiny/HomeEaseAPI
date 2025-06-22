using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.ProviderQueries;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Queries.ProviderQueries;

public class GetProviderByIdQuery : IRequest<ProviderDto>
{
    public Guid ProviderId { get; set; }
}

public class GetProviderByIdQueryHandler(IProviderRepository _providerRepository, IMapper _mapper) : IRequestHandler<GetProviderByIdQuery, ProviderDto>
{
    public async Task<ProviderDto> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdWithDetailsAsync(request.ProviderId);

        return _mapper.Map<ProviderDto>(provider);
    }
}