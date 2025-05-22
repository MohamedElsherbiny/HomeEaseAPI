using Massage.Application.Interfaces.Services;
using Massage.Infrastructure.Data;

namespace Massage.Infrastructure.Services;

public class UnitOfWork(AppDbContext _dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
