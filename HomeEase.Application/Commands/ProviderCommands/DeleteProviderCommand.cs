using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ProviderCommands;

public class DeleteProviderCommand : IRequest<EntityResult>
{
    public Guid ProviderId { get; set; }
}

public class DeleteProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteProviderCommand, EntityResult>
{
    public async Task<EntityResult> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId);
        if (provider is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ProviderNotFound), Messages.ProviderNotFound));
        }

        providerRepository.Delete(provider);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}