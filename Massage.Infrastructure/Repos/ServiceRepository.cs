using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Massage.Infrastructure.Services
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
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Service>> GetByProviderIdAsync(Guid providerId)
        {
            return await _dbContext.Services
                .Where(s => s.ProviderId == providerId)
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
    }
}
