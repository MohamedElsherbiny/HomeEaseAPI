using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Application.Queries.ProviderImages;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.ProviderImages
{
    public class GetProviderImageByIdQuery : IRequest<ProviderImageDto>
    {
        public Guid Id { get; set; }

        public GetProviderImageByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}



public class GetProviderImageByIdQueryHandler : IRequestHandler<GetProviderImageByIdQuery, ProviderImageDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetProviderImageByIdQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProviderImageDto> Handle(GetProviderImageByIdQuery request, CancellationToken cancellationToken)
    {
        var image = await _context.ProviderImages.FindAsync(request.Id);
        return image == null ? null : _mapper.Map<ProviderImageDto>(image);
    }
}