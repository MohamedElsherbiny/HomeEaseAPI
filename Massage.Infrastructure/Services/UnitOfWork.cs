using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;



namespace Massage.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
