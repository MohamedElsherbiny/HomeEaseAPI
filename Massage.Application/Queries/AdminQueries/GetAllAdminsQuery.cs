using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Queries.AdminQueries
{
    // Query to get all Admins
    public class GetAllAdminsQuery : IRequest<IEnumerable<UserDto>> { }

    public class GetAllAdminsQueryHandler : IRequestHandler<GetAllAdminsQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetAllAdminsQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
        {
            var (users, _) = await _userRepository.GetAllAsync(1, 1000, "", "", false, null);
            return users
                .Where(u => u.Role == UserRole.Admin)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.ToString(),
                    ProfileImageUrl = u.ProfileImageUrl
                });
        }
    }
}
