//  Command and Command Handlers for Image Upload (User & Provider)
using MediatR;
using Microsoft.AspNetCore.Http;
using Massage.Infrastructure.FileStorage;
using Massage.Domain.Entities;
using Massage.Application.Features.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Massage.Domain.Repositories;
using Massage.Application.Interfaces.Services;
using System.Text.RegularExpressions;



namespace Massage.Application.Features.Images
{
    // UserImage Command and Handler
    public class UpdateUserImageCommandHandler : IRequestHandler<UpdateUserImageCommand, UpdateUserImageResponse>
    {
        private readonly IUserRepository _repository;
        private readonly IFileStorageClient _fileStorageClient;

        public UpdateUserImageCommandHandler(IUserRepository repository, IFileStorageClient fileStorageClient)
        {
            _repository = repository;
            _fileStorageClient = fileStorageClient;
        }

        public async Task<UpdateUserImageResponse> Handle(UpdateUserImageCommand command, CancellationToken cancellationToken)
        {
            var user = await _repository.GetUserByIdAsync(command.UserId);
            if (user == null)
            {
                throw new UserNotFoundException($"User {command.UserId} not found");
            }

            var extension = Path.GetExtension(command.ProfileImage.FileName).ToLower();
            await using var fileStream = command.ProfileImage.OpenReadStream();
            var profileImageUrl = await _fileStorageClient.StoreFileAsync(
                fileStream,
                $"users/{user.Id}/{Guid.NewGuid()}{extension}"
            );

            user.ProfileImageUrl = profileImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

             _repository.Update(user);

            return new UpdateUserImageResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.ProfileImageUrl
            );
        }
    }

    public record UpdateUserImageCommand(
        Guid UserId,
        IFormFile ProfileImage
    ) : IRequest<UpdateUserImageResponse>;

    public record UpdateUserImageResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string ProfileImageUrl
    );

    // ProviderImage Command and Handler
    public class UpdateProviderImageCommandHandler : IRequestHandler<UpdateProviderImageCommand, UpdateProviderImageResponse>
    {
        private readonly IProviderRepository _repository;
        private readonly IFileStorageClient _fileStorageClient;

        public UpdateProviderImageCommandHandler(IProviderRepository repository, IFileStorageClient fileStorageClient)
        {
            _repository = repository;
            _fileStorageClient = fileStorageClient;
        }

        public async Task<UpdateProviderImageResponse> Handle(UpdateProviderImageCommand command, CancellationToken cancellationToken)
        {
            var provider = await _repository.GetByIdAsync(command.ProviderId);
            if (provider == null)
            {
                throw new ProviderNotFoundException($"Provider {command.ProviderId} not found");
            }

            // نظف اسم الملف الأصلي وProviderId
            var safeFileName = SanitizeFileName(command.ProfileImage.FileName);
            var safeProviderId = SanitizeSegment(provider.Id.ToString());

            // مسار الملف الآمن
            var blobPath = $"providers/{safeProviderId}/{safeFileName}";

            await using var fileStream = command.ProfileImage.OpenReadStream();
            var profileImageUrl = await _fileStorageClient.StoreFileAsync(fileStream, blobPath);

            provider.ProfileImageUrl = profileImageUrl;
            _repository.Update(provider);

            return new UpdateProviderImageResponse(
                provider.Id,
                provider.BusinessName,
                provider.ProfileImageUrl
            );
        }

        // دالة لتنظيف اسم الملف
        private string SanitizeFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower() ?? ".jpg";
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_\-]", "");
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "file";
            }

            return $"{baseName}_{Guid.NewGuid()}{extension}";
        }

        // دالة لتنظيف ProviderId أو أي جزء من المسار
        private string SanitizeSegment(string segment)
        {
            return Regex.Replace(segment, @"[^a-zA-Z0-9_\-]", "");
        }

    }

    public record UpdateProviderImageCommand(
        Guid ProviderId,
        IFormFile ProfileImage
    ) : IRequest<UpdateProviderImageResponse>;

    public record UpdateProviderImageResponse(
        Guid Id,
        string BusinessName,
        string ProfileImageUrl
    );

    // Exception classes
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }

    public class ProviderNotFoundException : Exception
    {
        public ProviderNotFoundException(string message) : base(message) { }
    }
}

// 4. File Storage Implementation - Keep using the existing Azure Blob Storage client

namespace Massage.Infrastructure.FileStorage
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

            Console.WriteLine("Uploading file with name: " + fileName); // للتصحيح لو حصلت مشكلة

            await _containerClient.CreateIfNotExistsAsync();
            var blobClient = _containerClient.GetBlobClient(fileName);
            fileStream.Position = 0;
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}

// 6. Service Registration in DI Container

namespace Massage.API
{
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
                cfg.RegisterServicesFromAssembly(typeof(Massage.Application.Features.Images.UpdateUserImageCommand).Assembly));

            return services;
        }
    }
}