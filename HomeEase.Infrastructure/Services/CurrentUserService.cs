using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using HomeEase.Application.Interfaces.Services;

namespace HomeEase.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor _httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? Guid.Parse(userId) : Guid.Empty;
        }
    }

    public string UserRole
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
