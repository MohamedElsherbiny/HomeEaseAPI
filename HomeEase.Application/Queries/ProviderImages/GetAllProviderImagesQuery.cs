using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Application.Queries.ProviderImages;
using MediatR;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.ProviderImages
{
    public class GetAllProviderImagesQuery : IRequest<List<ProviderImageDto>> { }
}


public class GetAllProviderImagesQueryHandler : IRequestHandler<GetAllProviderImagesQuery, List<ProviderImageDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetAllProviderImagesQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProviderImageDto>> Handle(GetAllProviderImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await _context.ProviderImages.ToListAsync();
        return _mapper.Map<List<ProviderImageDto>>(images);
    }
}