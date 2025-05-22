using HomeEase.Application.Interfaces.Services;
using HomeEase.Infrastructure.Data;

namespace HomeEase.Infrastructure.Services;

public class UnitOfWork(AppDbContext _dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
