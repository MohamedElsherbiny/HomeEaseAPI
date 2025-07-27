using HomeEase.Application.Interfaces;
using MediatR;

namespace HomeEase.Application.Commands.ProviderImages;

public class UpdateProviderImageCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateProviderImageCommandHandler(IAppDbContext _context) : IRequestHandler<UpdateProviderImageCommand, Unit>
{
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