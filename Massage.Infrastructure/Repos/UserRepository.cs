
using Massage.Application.DTOs;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Massage.Infrastructure.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            return user == null ? null : new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
                // Map other properties as needed
            };

        }

        public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, string searchTerm, string sortBy, bool sortDescending)
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

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public void Update(User user)
        {
            _dbContext.Users.Update(user);
        }
    }
}
