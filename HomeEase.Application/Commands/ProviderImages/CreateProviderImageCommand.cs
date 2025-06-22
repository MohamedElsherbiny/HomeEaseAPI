using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Commands.ProviderImages;

public class CreateProviderImageCommand : IRequest<ProviderImageDto>
{
    public Guid ProviderId { get; set; }
    public ImageType ImageType { get; set; } = ImageType.Gallery;
    public required string ImageUrl { get; set; }
}

public class CreateProviderImageCommandHandler(IAppDbContext _context, IMapper _mapper) : IRequestHandler<CreateProviderImageCommand, ProviderImageDto>
{
    public async Task<ProviderImageDto> Handle(CreateProviderImageCommand request, CancellationToken cancellationToken)
    {
        // If ImageType is Logo or Cover, ensure only one exists
        if (request.ImageType is ImageType.Logo or ImageType.Cover)
        {
            var existing = await _context.ProviderImages
                .Where(p => p.ProviderId == request.ProviderId && p.ImageType == request.ImageType)
                .ToListAsync(cancellationToken);

            if (existing.Any())
            {
                _context.ProviderImages.RemoveRange(existing);
            }
        }

        int sortOrder = 1;
        if (request.ImageType == ImageType.Gallery)
        {
            var maxSortOrder = await _context.ProviderImages
                .Where(p => p.ProviderId == request.ProviderId && p.ImageType == ImageType.Gallery)
                .MaxAsync(p => (int?)p.SortOrder, cancellationToken) ?? 0;

            sortOrder = maxSortOrder + 1;
        }

        var entity = new ProviderImage
        {
            Id = Guid.NewGuid(),
            ProviderId = request.ProviderId,
            SortOrder = sortOrder,
            ImageUrl = request.ImageUrl,
            ImageType = request.ImageType,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProviderImages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProviderImageDto>(entity);
    }
}
