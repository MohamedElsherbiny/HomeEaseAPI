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

    public string Language
    {
        get
        {
            var acceptLanguage = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();

            if (!string.IsNullOrWhiteSpace(acceptLanguage))
            {
                // Extract primary language from header like "en-US,en;q=0.9"
                var primary = acceptLanguage.Split(',').FirstOrDefault()?.Trim().ToLower();
                if (primary?.StartsWith("en") == true)
                    return "en";
                if (primary?.StartsWith("ar") == true)
                    return "ar";
            }

            // Default to "ar" if not found or invalid
            return "ar";
        }
    }
}
