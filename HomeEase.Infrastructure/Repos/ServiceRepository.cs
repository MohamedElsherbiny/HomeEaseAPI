using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace HomeEase.Infrastructure.Services
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _dbContext;

        public ServiceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Service> GetByIdAsync(Guid id)
        {
            return await _dbContext.Services
                .Include(x => x.BasePlatformService)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Service>> GetByProviderIdAsync(Guid providerId)
        {
            return await _dbContext.Services
                .Where(s => s.ProviderId == providerId)
                .Include(x => x.BasePlatformService)
                .ToListAsync();
        }

        public async Task AddAsync(Service service)
        {
            await _dbContext.Services.AddAsync(service);
        }

        public void Update(Service service)
        {
            _dbContext.Services.Update(service);
        }

        public void Delete(Service service)
        {
            _dbContext.Services.Remove(service);
        }

        public async Task<Service?> FindAsync(Expression<Func<Service, bool>> predicate)
        {
            return await _dbContext.Services
                .Include(x => x.BasePlatformService)
                .FirstOrDefaultAsync(predicate);
        }
    }
}
