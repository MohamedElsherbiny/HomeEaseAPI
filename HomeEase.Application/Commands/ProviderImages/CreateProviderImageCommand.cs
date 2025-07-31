using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Commands.ProviderImages;

public class CreateProviderImageCommand : IRequest<List<ProviderImageDto>>
{
    public Guid ProviderId { get; set; }
    public ImageType ImageType { get; set; } = ImageType.Gallery;
    public required List<string> ImageUrls { get; set; }
}


public class CreateProviderImageCommandHandler(IAppDbContext _context, IMapper _mapper)
    : IRequestHandler<CreateProviderImageCommand, List<ProviderImageDto>>
{
    public async Task<List<ProviderImageDto>> Handle(CreateProviderImageCommand request, CancellationToken cancellationToken)
    {
        var createdImages = new List<ProviderImage>();

        if (request.ImageType == ImageType.Logo || request.ImageType == ImageType.Cover)
        {

            var existing = await _context.ProviderImages
                .Where(p => p.ProviderId == request.ProviderId && p.ImageType == request.ImageType)
                .ToListAsync(cancellationToken);

            if (existing.Any())
            {
                _context.ProviderImages.RemoveRange(existing);
            }

            var entity = new ProviderImage
            {
                Id = Guid.NewGuid(),
                ProviderId = request.ProviderId,
                ImageUrl = request.ImageUrls.First(),
                ImageType = request.ImageType,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProviderImages.Add(entity);
            createdImages.Add(entity);
        }
        else if (request.ImageType == ImageType.Gallery)
        {
            var maxSortOrder = await _context.ProviderImages
                .Where(p => p.ProviderId == request.ProviderId && p.ImageType == ImageType.Gallery)
                .MaxAsync(p => (int?)p.SortOrder, cancellationToken) ?? 0;

            int sortOrder = maxSortOrder + 1;

            foreach (var imageUrl in request.ImageUrls)
            {
                var entity = new ProviderImage
                {
                    Id = Guid.NewGuid(),
                    ProviderId = request.ProviderId,
                    ImageUrl = imageUrl,
                    ImageType = ImageType.Gallery,
                    SortOrder = sortOrder++,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProviderImages.Add(entity);
                createdImages.Add(entity);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<List<ProviderImageDto>>(createdImages);
    }
}

