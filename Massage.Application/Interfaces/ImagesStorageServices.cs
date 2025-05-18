using Azure.Storage.Blobs;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;




public interface IFileStorageClient
{
    Task<string> StoreFileAsync(Stream fileStream, string fileName);
}

public class BlobContainerServiceClient : IFileStorageClient
{
    private readonly BlobContainerClient _containerClient;

    public BlobContainerServiceClient(BlobServiceClient blobServiceClient, string containerName)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> StoreFileAsync(Stream fileStream, string fileName)
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream), "File stream must not be null.");
        }

        Console.WriteLine("Uploading file with name: " + fileName);

        await _containerClient.CreateIfNotExistsAsync();
        var blobClient = _containerClient.GetBlobClient(fileName);
        fileStream.Position = 0;
        await blobClient.UploadAsync(fileStream, overwrite: true);

        return blobClient.Uri.ToString();
    }
}

public interface IFileStorageClientFactory
{
    IFileStorageClient GetClient(string containerName);
}

public class FileStorageClientFactory : IFileStorageClientFactory
{
    private readonly BlobServiceClient _blobServiceClient;

    public FileStorageClientFactory(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public IFileStorageClient GetClient(string containerName)
    {
        return new BlobContainerServiceClient(_blobServiceClient, containerName);
    }
}



public interface IBasePlatformServiceRepository
{
    Task<BasePlatformService> GetByIdAsync(Guid id);
    void Update(BasePlatformService service);
}

public class BasePlatformServiceRepository : IBasePlatformServiceRepository
{
    private readonly IAppDbContext _context;

    public BasePlatformServiceRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BasePlatformService> GetByIdAsync(Guid id)
    {
        return await _context.BasePlatformService.FindAsync(id);
    }

    public void Update(BasePlatformService service)
    {
        _context.BasePlatformService.Update(service);
    }
}