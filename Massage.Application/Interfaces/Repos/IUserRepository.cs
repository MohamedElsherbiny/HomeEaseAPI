using Massage.Application.DTOs;
using Massage.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces.Services
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(Guid userId);
        Task<(IEnumerable<User> users, int totalCount)> GetAllAsync(int page, int pageSize, string searchTerm, string sortBy, bool sortDescending, bool? isActive);
        void Update(User user);
    }
}
