using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IProviderService
    {
        Task<Provider> CreateProviderProfile(User user, string businessName, string businessAddress, string email,
            string description, string profileImageUrl, string businessNameAr, string descriptionAr,
            int experienceYears, string spokenLanguage,
            string street,
            string logoUrl,
            string coverUrl,
            List<string> images,
            decimal? lat,
            decimal? lng);
    }
}
