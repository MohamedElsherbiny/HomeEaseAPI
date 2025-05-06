using Massage.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId);
        Task<Address> GetByIdAsync(Guid addressId);
        Task AddAsync(Address address);
        void Update(Address address);
        void Delete(Address address);
    }
}
