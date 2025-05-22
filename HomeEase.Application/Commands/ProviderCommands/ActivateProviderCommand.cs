using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Exceptions;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class ActivateProviderCommand(Guid providerId) : IRequest<bool>
{
    public Guid ProviderId { get; } = providerId;
}

public class ActivateProviderCommandHandler(IProviderRepository _providerRepository, IUnitOfWork _unitOfWork) : IRequestHandler<ActivateProviderCommand, bool>
{
    public async Task<bool> Handle(ActivateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider is null)
        {
            throw new BusinessException($"Provider with ID {request.ProviderId} not found.");
        }

        provider.IsActive = true;
        provider.UpdatedAt = DateTime.UtcNow;
        provider.DeactivatedAt = null;

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}