using AutoMapper;
using Azure.Storage.Blobs;
using MediatR;
using Microsoft.Extensions.Configuration;
using HomeEase.Application.Interfaces;

namespace HomeEase.Application.Commands.ProviderImages;
public class DeleteProviderImageCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public DeleteProviderImageCommand(Guid id)
    {
        Id = id;
    }
}



public class DeleteProviderImageCommandHandler : IRequestHandler<DeleteProviderImageCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly BlobContainerClient _containerClient;

    public DeleteProviderImageCommandHandler(IAppDbContext context, IConfiguration config)
    {
        _context = context;
        var blobServiceClient = new BlobServiceClient(config["BlobStorage:ConnectionString"]);
        _containerClient = blobServiceClient.GetBlobContainerClient("general-files");
    }

    public async Task<Unit> Handle(DeleteProviderImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _context.ProviderImages.FindAsync(request.Id);
        if (image == null) throw new KeyNotFoundException("Image not found");

        try
        {
            var blobUri = new Uri(image.ImageUrl);
            var blobName = blobUri.AbsolutePath.TrimStart('/');
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Blob delete failed: {ex.Message}");
        }

        _context.ProviderImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
