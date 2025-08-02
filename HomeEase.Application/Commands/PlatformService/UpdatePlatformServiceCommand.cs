using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Resources;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Application.Commands.PlatformService
{
    public class UpdatePlatformServiceCommand : IRequest<EntityResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? NameAr { get; set; }
        public string Description { get; set; }
        public string? DescriptionAr { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UpdatePlatformServiceHandler(IAppDbContext context) : IRequestHandler<UpdatePlatformServiceCommand, EntityResult>
    {
        public async Task<EntityResult> Handle(UpdatePlatformServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await context.BasePlatformService.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (service is null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.PlatformServiceNotFound), Messages.PlatformServiceNotFound));
            }

            service.Name = request.Name;
            service.NameAr = request.NameAr;
            service.Description = request.Description;
            service.DescriptionAr = request.DescriptionAr;
            service.ImageUrl = request.ImageUrl;

            await context.SaveChangesAsync(cancellationToken);

            return EntityResult.Success;
        }
    }
}
