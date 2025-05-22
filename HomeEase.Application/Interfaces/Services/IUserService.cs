using HomeEase.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        
    }
}
