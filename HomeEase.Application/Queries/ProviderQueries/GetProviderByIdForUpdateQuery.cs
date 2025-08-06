using AutoMapper;
using HomeEase.Application.DTOs.Provider;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Queries.ProviderQueries;

public class GetProviderByIdForUpdateQuery : IRequest<ProviderForUpdateDto>
{
    public Guid ProviderId { get; set; }
}

public class GetProviderByIdForUpdateQueryHandler(IProviderRepository _providerRepository, IMapper _mapper) : IRequestHandler<GetProviderByIdForUpdateQuery, ProviderForUpdateDto>
{
    public async Task<ProviderForUpdateDto> Handle(GetProviderByIdForUpdateQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdWithDetailsAsync(request.ProviderId);

        return _mapper.Map<ProviderForUpdateDto>(provider);
    }
}