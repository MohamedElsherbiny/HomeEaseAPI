using Massage.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces
{
    public interface IUserPreferencesRepository
    {
        Task<UserPreferences> GetByUserIdAsync(Guid userId);
        Task AddAsync(UserPreferences preferences);
        void Update(UserPreferences preferences);
    }
}
