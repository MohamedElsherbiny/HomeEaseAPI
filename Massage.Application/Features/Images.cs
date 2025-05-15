// 2. Command and Command Handlers for Image Upload (User & Provider)
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

            var extension = Path.GetExtension(command.ProfileImage.FileName).ToLower();
            await using var fileStream = command.ProfileImage.OpenReadStream();
            var profileImageUrl = await _fileStorageClient.StoreFileAsync(
                fileStream,
                $"providers/{provider.Id}/{Guid.NewGuid()}{extension}"
            );

            provider.ProfileImageUrl = profileImageUrl;

             _repository.Update(provider);

            return new UpdateProviderImageResponse(
                provider.Id,
                provider.BusinessName,
                provider.ProfileImageUrl
            );
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

            await _containerClient.CreateIfNotExistsAsync();
            var blobClient = _containerClient.GetBlobClient(fileName);
            fileStream.Position = 0;
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}

// 5. Add API Controller Endpoints for User & Provider Image Upload

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{id}/profile-image")]
        public async Task<ActionResult<UpdateUserImageResponse>> UpdateUserImage(Guid id, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image file provided");
            }

            // Check file size (e.g., max 5MB)
            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size exceeds the limit (5MB)");
            }

            // Check file type
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return BadRequest("Only JPG, JPEG, and PNG files are allowed");
            }

            try
            {
                var command = new UpdateUserImageCommand(id, image);
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProvidersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{id}/profile-image")]
        public async Task<ActionResult<UpdateProviderImageResponse>> UpdateProviderImage(Guid id, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image file provided");
            }

            // Check file size (e.g., max 5MB)
            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size exceeds the limit (5MB)");
            }

            // Check file type
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return BadRequest("Only JPG, JPEG, and PNG files are allowed");
            }

            try
            {
                var command = new UpdateProviderImageCommand(id, image);
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ProviderNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
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
                    "massage-images"
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