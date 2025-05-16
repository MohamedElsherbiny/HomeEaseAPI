using Azure.Storage.Blobs;
using Massage.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces
{
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
}


// Service Registration in DI Container
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Azure Blob Storage client
        services.AddSingleton(x => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));
        services.AddSingleton<IFileStorageClient>(provider =>
            new BlobContainerServiceClient(
                provider.GetRequiredService<BlobServiceClient>(),
                "images"
            )
        );

        return services;
    }

    public static IServiceCollection AddMediatRHandlers(this IServiceCollection services)
    {
        // Register MediatR and all handlers in the specified assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Massage.Application.Commands.UserCommends.UpdateUserImageCommand).Assembly));

        return services;
    }
}