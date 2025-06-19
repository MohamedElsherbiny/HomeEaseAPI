using AutoMapper;
using Azure.Storage.Blobs;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.RegularExpressions;

namespace HomeEase.Application.Commands.ProviderImages;

public class CreateProviderImageCommand : IRequest<ProviderImageDto>
{
    public Guid ProviderId { get; set; }
    public int SortOrder { get; set; }
    public IFormFile ImageFile { get; set; } = default!;
}


public class CreateProviderImageCommandHandler : IRequestHandler<CreateProviderImageCommand, ProviderImageDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly BlobContainerClient _containerClient;

    public CreateProviderImageCommandHandler(IAppDbContext context, IMapper mapper, IConfiguration config)
    {
        _context = context;
        _mapper = mapper;
        var blobServiceClient = new BlobServiceClient(config["BlobStorage:ConnectionString"]);
        _containerClient = blobServiceClient.GetBlobContainerClient("general-files");
        _containerClient.CreateIfNotExists();
    }

    public async Task<ProviderImageDto> Handle(CreateProviderImageCommand request, CancellationToken cancellationToken)
    {
        var fileName = SanitizeFileName(request.ImageFile.FileName);
        var blobPath = $"provider-images/{Guid.NewGuid()}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobPath);

        await using var stream = request.ImageFile.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        var entity = new ProviderImage
        {
            Id = Guid.NewGuid(),
            ProviderId = request.ProviderId,
            SortOrder = request.SortOrder,
            ImageUrl = blobClient.Uri.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ProviderImages.Add(entity);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProviderImageDto>(entity);
    }

    private string SanitizeFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLower() ?? "";
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_\-]", "");
        if (string.IsNullOrWhiteSpace(baseName)) baseName = "file";
        return $"{baseName}_{Guid.NewGuid()}{extension}";
    }
}