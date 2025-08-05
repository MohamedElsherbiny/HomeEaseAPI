using HomeEase.Domain.Enums;

namespace HomeEase.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserRole { get; }
    LanguageEnum Language { get; }
}
