using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.AdminCommands
{
    // Command to delete an Admin
    public class DeleteAdminCommand : IRequest<bool>
    {
        public Guid AdminId { get; set; }

        public DeleteAdminCommand(Guid adminId)
        {
            AdminId = adminId;
        }
    }

    public class DeleteAdminCommandHandler : IRequestHandler<DeleteAdminCommand, bool>
    {
        private readonly UserManager<User> _userManager;

        public DeleteAdminCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.AdminId.ToString());

            if (user == null)
                throw new Exception("User not found.");

            if (user.Role != UserRole.Admin)
                throw new Exception("User is not an admin.");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to delete admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return true;
        }
    }
}
