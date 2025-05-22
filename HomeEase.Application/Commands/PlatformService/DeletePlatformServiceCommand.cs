using HomeEase.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.PlatformService
{
    public class DeletePlatformServiceCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }


    public class DeletePlatformServiceHandler : IRequestHandler<DeletePlatformServiceCommand, bool>
    {
        private readonly IAppDbContext _context;
        public DeletePlatformServiceHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeletePlatformServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.BasePlatformService.FindAsync(new object[] { request.Id }, cancellationToken);
            if (service == null) return false;

            _context.BasePlatformService.Remove(service);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
