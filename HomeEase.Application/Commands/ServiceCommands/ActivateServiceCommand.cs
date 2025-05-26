using AutoMapper;
using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.ServiceCommands
{
    public class ActivateServiceCommand : IRequest<ServiceDto>
    {
        public Guid ServiceId { get; set; }
        public Guid ProviderId { get; set; } 
    }
}

// Command Handler
public class ActivateServiceHandler : IRequestHandler<ActivateServiceCommand, ServiceDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public ActivateServiceHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceDto> Handle(ActivateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.ProviderId == request.ProviderId, cancellationToken);

        if (service == null)
        {
            throw new Exception("Service not found or you do not have permission to modify it.");
        }

        if (service.IsActive)
        {
            return _mapper.Map<ServiceDto>(service); 
        }

        service.IsActive = true;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ServiceDto>(service);
    }
}