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
    public class DeactivateServiceCommand : IRequest<ServiceDto>
    {
        public Guid ServiceId { get; set; }
        public Guid ProviderId { get; set; } 
    }
}


// Command Handler
public class DeactivateServiceHandler : IRequestHandler<DeactivateServiceCommand, ServiceDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public DeactivateServiceHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceDto> Handle(DeactivateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.ProviderId == request.ProviderId, cancellationToken);

        if (service == null)
        {
            throw new Exception("Service not found or you do not have permission to modify it.");
        }

        if (!service.IsActive)
        {
            return _mapper.Map<ServiceDto>(service); // Already inactive, return as is
        }

        service.IsActive = false;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ServiceDto>(service);
    }
}