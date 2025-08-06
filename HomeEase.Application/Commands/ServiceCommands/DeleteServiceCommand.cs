using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ServiceCommands;

public class DeleteServiceCommand : IRequest<EntityResult>
{
    public Guid ServiceId { get; set; }
}

public class DeleteServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteServiceCommand, EntityResult>
{
    public async Task<EntityResult> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.ServiceId);
        if (service is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ServiceNotFound), Messages.ServiceNotFound));
        }

        serviceRepository.Delete(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}