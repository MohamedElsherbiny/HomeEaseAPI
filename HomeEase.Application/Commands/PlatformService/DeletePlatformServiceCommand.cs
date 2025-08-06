using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces;
using HomeEase.Resources;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Commands.PlatformService
{
    public class DeletePlatformServiceCommand : IRequest<EntityResult>
    {
        public Guid Id { get; set; }
    }

    public class DeletePlatformServiceHandler(IAppDbContext context) : IRequestHandler<DeletePlatformServiceCommand, EntityResult>
    {
        public async Task<EntityResult> Handle(DeletePlatformServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await context.BasePlatformService.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (service is null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.PlatformServiceNotFound), Messages.PlatformServiceNotFound));
            }

            context.BasePlatformService.Remove(service);
            await context.SaveChangesAsync(cancellationToken);

            return EntityResult.Success;
        }
    }
}
