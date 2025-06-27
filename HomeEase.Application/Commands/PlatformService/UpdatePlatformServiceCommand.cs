using HomeEase.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.PlatformService
{
    public class UpdatePlatformServiceCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? NameAr { get; set; }
        public string Description { get; set; }
        public string? DescriptionAr { get; set; }
        public string ImageUrl { get; set; }
    }


    public class UpdatePlatformServiceHandler : IRequestHandler<UpdatePlatformServiceCommand, bool>
    {
        private readonly IAppDbContext _context;
        public UpdatePlatformServiceHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdatePlatformServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.BasePlatformService.FindAsync(new object[] { request.Id }, cancellationToken);
            if (service == null) return false;

            service.Name = request.Name;
            service.NameAr = request.NameAr;
            service.Description = request.Description;
            service.DescriptionAr = request.DescriptionAr;
            service.ImageUrl = request.ImageUrl;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
