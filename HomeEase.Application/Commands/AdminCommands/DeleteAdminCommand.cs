using HomeEase.Application.DTOs.Common;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeEase.Application.Commands.AdminCommands
{
    public class DeleteAdminCommand(Guid adminId) : IRequest<EntityResult>
    {
        public Guid AdminId { get; set; } = adminId;
    }

    public class DeleteAdminCommandHandler(UserManager<User> userManager) : IRequestHandler<DeleteAdminCommand, EntityResult>
    {
        public async Task<EntityResult> Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.AdminId.ToString());

            if (user == null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFound), string.Format(Messages.UserNotFound, request.AdminId.ToString())));
            }

            if (user.Role != UserRole.Admin)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.UserNotAdmin), Messages.UserNotAdmin));
            }

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errorDetails = string.Join(", ", result.Errors.Select(e => e.Description));

                return EntityResult.Failed(new EntityError(
                    nameof(Messages.DeleteAdminFailed),
                    string.Format(Messages.DeleteAdminFailed, errorDetails)));
            }

            return EntityResult.Success;
        }
    }
}
