using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Massage.Application.Commands
{
    public record UpdateBasePlatformServiceImageCommand(
        Guid ServiceId,
        IFormFile Image
    ) : IRequest<UpdateBasePlatformServiceImageResponse>;

    public record UpdateBasePlatformServiceImageResponse(
        Guid Id,
        string Name,
        string ImageUrl
    );

    public class UpdateBasePlatformServiceImageCommandHandler : IRequestHandler<UpdateBasePlatformServiceImageCommand, UpdateBasePlatformServiceImageResponse>
    {
        private readonly IBasePlatformServiceRepository _repository;
        private readonly IFileStorageClientFactory _fileStorageClientFactory;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBasePlatformServiceImageCommandHandler(
            IBasePlatformServiceRepository repository,
            IFileStorageClientFactory fileStorageClientFactory,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _fileStorageClientFactory = fileStorageClientFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateBasePlatformServiceImageResponse> Handle(UpdateBasePlatformServiceImageCommand command, CancellationToken cancellationToken)
        {
            var service = await _repository.GetByIdAsync(command.ServiceId);
            if (service == null)
            {
                throw new NotFoundException($"BasePlatformService with ID {command.ServiceId} not found");
            }

            var safeFileName = SanitizeFileName(command.Image.FileName);
            var safeServiceId = SanitizeSegment(service.Id.ToString());

            var blobPath = $"services/{safeServiceId}/{safeFileName}";

            await using var fileStream = command.Image.OpenReadStream();
            var fileStorageClient = _fileStorageClientFactory.GetClient("service-images");
            var imageUrl = await fileStorageClient.StoreFileAsync(fileStream, blobPath);

            service.ImageUrl = imageUrl;
            service.UpdatedAt = DateTime.UtcNow;

            _repository.Update(service);
            await _unitOfWork.SaveChangesAsync();

            return new UpdateBasePlatformServiceImageResponse(
                service.Id,
                service.Name,
                service.ImageUrl
            );
        }

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

        private string SanitizeSegment(string segment)
        {
            return Regex.Replace(segment, @"[^a-zA-Z0-9_\-]", "");
        }
    }
}