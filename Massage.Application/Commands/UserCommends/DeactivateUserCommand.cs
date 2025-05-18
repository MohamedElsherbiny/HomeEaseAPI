using Massage.Application.Commands.UserCommends;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.UserCommends
{
    public enum EntityType
    {
        User,
        Provider
    }

    public class DeactivateCommand : IRequest<bool>
    {
        public Guid Id { get; }
        public EntityType EntityType { get; }

        public DeactivateCommand(Guid id, EntityType entityType)
        {
            Id = id;
            EntityType = entityType;
        }
    }
}


// Command Handler
public class DeactivateCommandHandler : IRequestHandler<DeactivateCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCommandHandler(
        IUserRepository userRepository,
        IProviderRepository providerRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeactivateCommand request, CancellationToken cancellationToken)
    {
        switch (request.EntityType)
        {
            case EntityType.User:
                var user = await _userRepository.GetUserByIdAsync(request.Id);
                if (user == null)
                    throw new NotFoundException($"User with ID {request.Id} not found.");

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                user.DeactivatedAt = DateTime.UtcNow;

                _userRepository.Update(user);
                break;

            case EntityType.Provider:
                var provider = await _providerRepository.GetByIdAsync(request.Id);
                if (provider == null)
                    throw new NotFoundException($"Provider with ID {request.Id} not found.");

                provider.IsActive = false;
                provider.UpdatedAt = DateTime.UtcNow;
                provider.DeactivatedAt = DateTime.UtcNow;

                _providerRepository.Update(provider);
                break;

            default:
                throw new BadRequestException("Invalid entity type.");
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}