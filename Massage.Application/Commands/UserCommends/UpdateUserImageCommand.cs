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

    public class UpdateUserImageCommandHandler : IRequestHandler<UpdateUserImageCommand, UpdateUserImageResponse>
    {
        private readonly IUserRepository _repository;
        private readonly IFileStorageClientFactory _fileStorageClientFactory;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserImageCommandHandler(
            IUserRepository repository,
            IFileStorageClientFactory fileStorageClientFactory,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _fileStorageClientFactory = fileStorageClientFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateUserImageResponse> Handle(UpdateUserImageCommand command, CancellationToken cancellationToken)
        {
            var user = await _repository.GetUserByIdAsync(command.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {command.UserId} not found");
            }

            var safeFileName = SanitizeFileName(command.ProfileImage.FileName);
            var safeUserId = SanitizeSegment(user.Id.ToString());

            var blobPath = $"users/{safeUserId}/{safeFileName}";

            await using var fileStream = command.ProfileImage.OpenReadStream();
            var fileStorageClient = _fileStorageClientFactory.GetClient("user-images");
            var profileImageUrl = await fileStorageClient.StoreFileAsync(fileStream, blobPath);

            user.ProfileImageUrl = profileImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new UpdateUserImageResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.ProfileImageUrl
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