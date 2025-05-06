using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Massage.Infrastructure.Repos
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _dbContext;

        public AddressRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Addresses.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<Address> GetByIdAsync(Guid addressId)
        {
            return await _dbContext.Addresses.FindAsync(addressId);
        }

        public async Task AddAsync(Address address)
        {
            await _dbContext.Addresses.AddAsync(address);
        }

        public void Update(Address address)
        {
            _dbContext.Addresses.Update(address);
        }

        public void Delete(Address address)
        {
            _dbContext.Addresses.Remove(address);
        }
    }
}
