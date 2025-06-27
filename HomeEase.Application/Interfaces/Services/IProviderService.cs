using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IProviderService
    {
        Task CreateProviderProfile(Guid userId, string businessName, string businessAddress, string email,
            string description, string profileImageUrl, string businessNameAr, string descriptionAr,
            int experienceYears, string spokenLanguage);
    }
}
