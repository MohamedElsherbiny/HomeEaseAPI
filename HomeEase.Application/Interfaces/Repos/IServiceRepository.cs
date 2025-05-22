using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeEase.Domain.Entities;

namespace HomeEase.Domain.Repositories
{
    public interface IServiceRepository
    {
        Task<Service> GetByIdAsync(Guid id);
        Task<List<Service>> GetByProviderIdAsync(Guid providerId);
        Task AddAsync(Service service);
        void Update(Service service);
        void Delete(Service service);
    }
}
