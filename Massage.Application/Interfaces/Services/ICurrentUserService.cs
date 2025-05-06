using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string UserRole { get; }
    }
}
