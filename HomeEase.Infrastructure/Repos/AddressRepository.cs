using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Repos;

public class AddressRepository(AppDbContext _dbContext) : IAddressRepository
{
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
