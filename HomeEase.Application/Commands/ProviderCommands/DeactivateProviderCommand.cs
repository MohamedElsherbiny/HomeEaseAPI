using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Exceptions;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class DeactivateProviderCommand(Guid providerId) : IRequest<bool>
{
    public Guid ProviderId { get; } = providerId;
}

public class DeactivateProviderCommandHandler(IProviderRepository _providerRepository, IUnitOfWork _unitOfWork) : IRequestHandler<DeactivateProviderCommand, bool>
{
    public async Task<bool> Handle(DeactivateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
        {
            throw new BusinessException($"Provider with ID {request.ProviderId} not found.");
        }

        provider.IsActive = false;
        provider.UpdatedAt = DateTime.UtcNow;
        provider.DeactivatedAt = DateTime.UtcNow;

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}