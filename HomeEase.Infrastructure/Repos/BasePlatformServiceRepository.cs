using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Repos
{
    public class BasePlatformServiceRepository : IBasePlatformServiceRepository
    {
        private readonly IAppDbContext _context;

        public BasePlatformServiceRepository(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<BasePlatformService> GetByIdAsync(Guid id)
        {
            return await _context.BasePlatformService
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
