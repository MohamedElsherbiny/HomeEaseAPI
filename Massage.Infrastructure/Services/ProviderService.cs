using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Enums;
using Massage.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Infrastructure.Services
{
    public class ProviderService : IProviderService
    {
        private readonly AppDbContext _context;

        public ProviderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateProviderProfile(Guid userId, string businessName, string businessAddress, string Email, string description, string profileImageUrl, string[] serviceTypes)
        {
            var provider = new Provider
            {
                Id = userId,
                UserId = userId,     
                Email = Email,
                BusinessName = businessName,
                BusinessAddress = businessAddress,
                Description = description,
                ProfileImageUrl = profileImageUrl,
                ServiceTypes = serviceTypes.ToList(),
                Status = ProviderStatus.Pending,
                Rating = 0
            };

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();
        }
    }
}
