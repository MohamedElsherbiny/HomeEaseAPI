using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Services;

public class UserService(AppDbContext _context) : IUserService
{
    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user =  _context.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };
    }
}
