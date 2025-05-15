using MediatR;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Exceptions;
using Massage.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Massage.Application.Interfaces.Services;
using AutoMapper;
using Massage.Application.Interfaces;
using Massage.Application.DTOs;

namespace Massage.Application.Commands.AdminCommands
{
    // Command to block a user
    public class BlockUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }

    public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BlockUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

    // Command to unblock a user
    public class UnblockUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }

    public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UnblockUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

}
