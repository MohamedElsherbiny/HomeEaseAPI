using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class UserServiceLikeRepository : IUserServiceLikeRepository
{
    private readonly AppDbContext _context;

    public UserServiceLikeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserServiceLike>> GetAllAsync(Guid? userId = null, Guid? serviceId = null)
    {
        var query = _context.UserServiceLikes.AsQueryable();

        if (userId.HasValue)
            query = query.Where(like => like.UserId == userId.Value);

        if (serviceId.HasValue)
            query = query.Where(like => like.ServiceId == serviceId.Value);

        return await query.ToListAsync();
    }

    public async Task<UserServiceLike?> GetByIdAsync(Guid id)
    {
        return await _context.UserServiceLikes.FindAsync(id);
    }

    public async Task AddAsync(UserServiceLike like)
    {
        await _context.UserServiceLikes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserServiceLike like)
    {
        _context.UserServiceLikes.Remove(like);
        await _context.SaveChangesAsync();
    }
}
