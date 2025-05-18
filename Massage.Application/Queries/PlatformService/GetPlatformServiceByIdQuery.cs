using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.PlatformService
{
    public class GetPlatformServiceByIdQuery : IRequest<BasePlatformService>
    {
        public Guid Id { get; set; }
    }


    public class GetPlatformServiceByIdHandler : IRequestHandler<GetPlatformServiceByIdQuery, BasePlatformService>
    {
        private readonly IAppDbContext _context;
        public GetPlatformServiceByIdHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<BasePlatformService> Handle(GetPlatformServiceByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.BasePlatformService.FindAsync(new object[] { request.Id }, cancellationToken);
        }
    }
}
