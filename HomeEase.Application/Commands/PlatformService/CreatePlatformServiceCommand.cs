﻿using HomeEase.Application.Interfaces;
using MediatR;
using HomeEase.Domain.Entities;

namespace HomeEase.Application.Commands.PlatformService
{
    public class CreatePlatformServiceCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string? DescriptionAr { get; set; }
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
                Description = request.Description,
                DescriptionAr = request.DescriptionAr,
                NameAr = request.NameAr,
                ImageUrl = request.ImageUrl
            };

            _context.BasePlatformService.Add(service);
            await _context.SaveChangesAsync(cancellationToken);
            return service.Id;
        }
    }
}