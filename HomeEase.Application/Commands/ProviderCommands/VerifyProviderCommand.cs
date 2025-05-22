using HomeEase.Application.Commands.ProviderCommands;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.ProviderCommands
{
    public class VerifyProviderCommand : IRequest<bool>
    {
        public Guid ProviderId { get; set; }
    }
}


// Command Handler
public class VerifyProviderCommandHandler : IRequestHandler<VerifyProviderCommand, bool>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyProviderCommandHandler(IProviderRepository providerRepository, IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(VerifyProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId);
        if (provider == null)
            return false;

        provider.Status = ProviderStatus.Suspended;
        provider.VerifiedAt = DateTime.UtcNow;

        _providerRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}