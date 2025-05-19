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
    public class ActivateProviderCommand : IRequest<bool>
    {
        public Guid ProviderId { get; }

        public ActivateProviderCommand(Guid providerId)
        {
            ProviderId = providerId;
        }
    }
}


// Command Handler
public class ActivateProviderCommandHandler : IRequestHandler<ActivateProviderCommand, bool>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ActivateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
            throw new NotFoundException($"Provider with ID {request.ProviderId} not found.");

        provider.IsActive = true;
        provider.UpdatedAt = DateTime.UtcNow;
        provider.DeactivatedAt = null;

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}