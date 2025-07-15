using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HomeEase.Domain.Repositories
{
    public interface IServiceRepository
    {
        Task<Service> GetByIdAsync(Guid id);
        Task<List<Service>> GetByProviderIdAsync(Guid providerId);
        Task AddAsync(Service service);
        void Update(Service service);
        void Delete(Service service);
        Task<Service?> FindAsync(Expression<Func<Service, bool>> predicate);
    }
}
