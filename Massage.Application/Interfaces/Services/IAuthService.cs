using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces.Services
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string passwordHash, string password);
        string GenerateJwtToken(Guid userId, string email, string role);
        Task<(bool Success, string Token, string RefreshToken)> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }
}
