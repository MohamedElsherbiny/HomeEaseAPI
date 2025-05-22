using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Massage.Infrastructure.Repos
{
    public class UserPreferencesRepository(AppDbContext _dbContext) : IUserPreferencesRepository
    {
        public async Task<UserPreferences> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task AddAsync(UserPreferences preferences)
        {
            await _dbContext.UserPreferences.AddAsync(preferences);
        }

        public void Update(UserPreferences preferences)
        {
            _dbContext.UserPreferences.Update(preferences);
        }
    }
}
