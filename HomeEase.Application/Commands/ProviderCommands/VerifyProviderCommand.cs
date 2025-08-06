using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class VerifyProviderCommand : IRequest<EntityResult>
{
    public Guid ProviderId { get; set; }
}

public class VerifyProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork) : IRequestHandler<VerifyProviderCommand, EntityResult>
{
    public async Task<EntityResult> Handle(VerifyProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId);
        if (provider is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ProviderNotFound), Messages.ProviderNotFound));
        }

        provider.Status = ProviderStatus.Suspended;
        provider.VerifiedAt = DateTime.UtcNow;

        providerRepository.Update(provider);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}