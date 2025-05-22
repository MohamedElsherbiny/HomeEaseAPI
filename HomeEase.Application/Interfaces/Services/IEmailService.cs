using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string token);
    }
}
