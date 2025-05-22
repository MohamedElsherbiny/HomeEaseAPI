using System.Threading;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}