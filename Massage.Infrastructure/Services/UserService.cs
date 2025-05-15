using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Massage.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

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
                // Add more properties if needed
            };
        }
    }
}
