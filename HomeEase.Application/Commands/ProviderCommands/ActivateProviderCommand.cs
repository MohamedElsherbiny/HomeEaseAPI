using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class ActivateProviderCommand(Guid providerId) : IRequest<EntityResult>
{
    public Guid ProviderId { get; } = providerId;
}

public class ActivateProviderCommandHandler(IProviderRepository _providerRepository, IUnitOfWork _unitOfWork) : IRequestHandler<ActivateProviderCommand, EntityResult>
{
    public async Task<EntityResult> Handle(ActivateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider is null)
        {
            return EntityResult.Failed(new EntityError(
                    nameof(Messages.ProviderNotFound),
                    string.Format(Messages.ProviderNotFound, request.ProviderId)));
        }

        provider.IsActive = true;
        provider.UpdatedAt = DateTime.UtcNow;
        provider.DeactivatedAt = null;

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}