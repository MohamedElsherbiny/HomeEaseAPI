using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Repos
{
    public interface IBasePlatformServiceRepository
    {
        Task<BasePlatformService> GetByIdAsync(Guid id);
    }
}
