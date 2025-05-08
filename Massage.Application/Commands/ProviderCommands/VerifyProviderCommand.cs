using Massage.Application.Commands.ProviderCommands;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Enums;
using Massage.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.ProviderCommands
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