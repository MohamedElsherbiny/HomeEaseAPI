using AutoMapper;
using HomeEase.Application.DTOs.Provider;
using HomeEase.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Queries.ProviderQueries;

public class GetProviderByIdQuery : IRequest<ProviderDto>
{
    public Guid ProviderId { get; set; }
}

public class GetProviderByIdQueryHandler(IAppDbContext _dbContext, IMapper _mapper) : IRequestHandler<GetProviderByIdQuery, ProviderDto>
{
    public async Task<ProviderDto> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _dbContext.Providers
            .Include(p => p.Address)
            .Include(p => p.User)
            .Include(p => p.Schedule)
                .ThenInclude(s => s.RegularHours)
            .Include(p => p.Schedule)
                .ThenInclude(s => s.SpecialDates)
            .Include(p => p.Schedule)
                .ThenInclude(s => s.AvailableSlots)
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId);

        if (provider is null)
        {
            return null;
        }

        provider.Services = [.. provider.Services.Where(s => s.Price > 0 || s.HomePrice > 0)];


        return _mapper.Map<ProviderDto>(provider);
    }
}