using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
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
    public record UpdateProviderImageCommand(
        Guid ProviderId,
        IFormFile ProfileImage
    ) : IRequest<UpdateProviderImageResponse>;

    public record UpdateProviderImageResponse(
        Guid Id,
        string BusinessName,
        string ProfileImageUrl
    );

    public class UpdateProviderImageCommandHandler : IRequestHandler<UpdateProviderImageCommand, UpdateProviderImageResponse>
    {
        private readonly IProviderRepository _repository;
        private readonly IFileStorageClientFactory _fileStorageClientFactory;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProviderImageCommandHandler(
            IProviderRepository repository,
            IFileStorageClientFactory fileStorageClientFactory,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _fileStorageClientFactory = fileStorageClientFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateProviderImageResponse> Handle(UpdateProviderImageCommand command, CancellationToken cancellationToken)
        {
            var provider = await _repository.GetByIdAsync(command.ProviderId);
            if (provider == null)
            {
                throw new NotFoundException($"Provider with ID {command.ProviderId} not found");
            }

            var safeFileName = SanitizeFileName(command.ProfileImage.FileName);
            var safeProviderId = SanitizeSegment(provider.Id.ToString());

            var blobPath = $"providers/{safeProviderId}/{safeFileName}";

            await using var fileStream = command.ProfileImage.OpenReadStream();
            var fileStorageClient = _fileStorageClientFactory.GetClient("provider-images");
            var profileImageUrl = await fileStorageClient.StoreFileAsync(fileStream, blobPath);

            provider.ProfileImageUrl = profileImageUrl;
            provider.UpdatedAt = DateTime.UtcNow;

            _repository.Update(provider);
            await _unitOfWork.SaveChangesAsync();

            return new UpdateProviderImageResponse(
                provider.Id,
                provider.BusinessName,
                provider.ProfileImageUrl
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