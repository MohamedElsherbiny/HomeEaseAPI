using Massage.Application.Exceptions;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.UserCommends
{
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
}
