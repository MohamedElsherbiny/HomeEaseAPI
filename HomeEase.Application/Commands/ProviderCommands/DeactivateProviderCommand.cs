using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class DeactivateProviderCommand(Guid providerId) : IRequest<EntityResult>
{
    public Guid ProviderId { get; } = providerId;
}

public class DeactivateProviderCommandHandler(IProviderRepository _providerRepository, IUnitOfWork _unitOfWork) : IRequestHandler<DeactivateProviderCommand, EntityResult>
{
    public async Task<EntityResult> Handle(DeactivateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
        {
            return EntityResult.Failed(new EntityError(
                    nameof(Messages.ProviderNotFound),
                    string.Format(Messages.ProviderNotFound, request.ProviderId)));
        }

        provider.IsActive = false;
        provider.UpdatedAt = DateTime.UtcNow;
        provider.DeactivatedAt = DateTime.UtcNow;

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}