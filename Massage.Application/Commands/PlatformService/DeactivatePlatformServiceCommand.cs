using AutoMapper;
using Massage.Application.Commands.PlatformService;
using Massage.Application.DTOs;
using Massage.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Commands.PlatformService
{
    public class DeactivatePlatformServiceCommand : IRequest<BasePlatformServiceDto>
    {
        public Guid Id { get; set; }
    }
}


// Command Handler
public class DeactivatePlatformServiceHandler : IRequestHandler<DeactivatePlatformServiceCommand, BasePlatformServiceDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public DeactivatePlatformServiceHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BasePlatformServiceDto> Handle(DeactivatePlatformServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.BasePlatformService
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            throw new Exception("Platform service not found.");
        }

        if (service.IsActive)
        {
            service.IsActive = false;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<BasePlatformServiceDto>(service);
    }
}