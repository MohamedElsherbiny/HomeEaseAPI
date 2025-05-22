using HomeEase.Application.Interfaces;
using MediatR;
using HomeEase.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.PlatformService
{
    public class CreatePlatformServiceCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }


    public class CreatePlatformServiceHandler : IRequestHandler<CreatePlatformServiceCommand, Guid>
    {
        private readonly IAppDbContext _context;
        public CreatePlatformServiceHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreatePlatformServiceCommand request, CancellationToken cancellationToken)
        {
            var service = new BasePlatformService
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ImageUrl = request.ImageUrl
            };

            _context.BasePlatformService.Add(service);
            await _context.SaveChangesAsync(cancellationToken);
            return service.Id;
        }
    }
}
