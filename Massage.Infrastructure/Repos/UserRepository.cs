using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Massage.Infrastructure.Repos;

public class UserRepository(AppDbContext _dbContext) : IUserRepository
{
    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    public async Task<(IEnumerable<User> users, int totalCount)> GetAllAsync(int page, int pageSize, string searchTerm, string sortBy, bool sortDescending, bool? isActive)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => u.FirstName.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        int totalCount = await query.CountAsync();

        var paginatedUsers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (paginatedUsers, totalCount);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
        _dbContext.SaveChanges();
    }
}
