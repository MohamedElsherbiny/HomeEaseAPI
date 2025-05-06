using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces.Services
{
    public interface IProviderService
    {
        Task CreateProviderProfile(Guid userId, string businessName,string businessAddress, string Email, string description, string profileImageUrl, string[] serviceTypes);
    }
}
