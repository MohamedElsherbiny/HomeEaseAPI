using AutoMapper;
using Azure.Storage.Blobs;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using MediatR;

using Microsoft.Extensions.Configuration;
using System;
using System.Text.RegularExpressions;

namespace HomeEase.Application.Commands.ProviderImages;

public class UpdateProviderImageCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}



public class UpdateProviderImageCommandHandler : IRequestHandler<UpdateProviderImageCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UpdateProviderImageCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateProviderImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _context.ProviderImages.FindAsync(request.Id);
        if (image == null) throw new KeyNotFoundException("Image not found");

        image.SortOrder = request.SortOrder;
        image.UpdatedAt = DateTime.UtcNow;

        _context.ProviderImages.Update(image);
        await _context.SaveChangesAsync();

        return Unit.Value;
    }
}