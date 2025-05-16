using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
using Massage.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Massage.Application.Commands.ProviderCommands
{
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


            var safeFileName = SanitizeFileName(command.ProfileImage.FileName);
            var safeProviderId = SanitizeSegment(provider.Id.ToString());

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

    public record UpdateProviderImageCommand(
        Guid ProviderId,
        IFormFile ProfileImage
    ) : IRequest<UpdateProviderImageResponse>;

    public record UpdateProviderImageResponse(
        Guid Id,
        string BusinessName,
        string ProfileImageUrl
    );
}
