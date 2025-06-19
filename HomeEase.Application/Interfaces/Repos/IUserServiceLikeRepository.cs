using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces
{
    public interface IUserServiceLikeRepository
    {
        Task<List<UserServiceLike>> GetAllAsync(Guid? userId = null, Guid? serviceId = null);
        Task<UserServiceLike?> GetByIdAsync(Guid id);
        Task AddAsync(UserServiceLike like);
        Task DeleteAsync(UserServiceLike like);
    }
}
