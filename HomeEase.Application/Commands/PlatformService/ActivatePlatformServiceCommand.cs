using AutoMapper;
using HomeEase.Application.Commands.PlatformService;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.PlatformService
{
    public class ActivatePlatformServiceCommand : IRequest<BasePlatformServiceDto>
    {
        public Guid Id { get; set; }
    }
}



public class ActivatePlatformServiceHandler : IRequestHandler<ActivatePlatformServiceCommand, BasePlatformServiceDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public ActivatePlatformServiceHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BasePlatformServiceDto> Handle(ActivatePlatformServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.BasePlatformService
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            throw new Exception("Platform service not found.");
        }

        if (!service.IsActive)
        {
            service.IsActive = true;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<BasePlatformServiceDto>(service);
    }
}