using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IJwtService
    {
        (string Token, DateTime Expiration) GenerateToken(User user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
