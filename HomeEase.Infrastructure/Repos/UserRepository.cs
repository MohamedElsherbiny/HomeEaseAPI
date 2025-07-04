﻿using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Repos;

public class UserRepository(AppDbContext _dbContext) : IUserRepository
{
    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    public async Task<(IEnumerable<User> users, int totalCount)> GetAllAsync(
        int page,
        int pageSize,
        string searchTerm,
        string sortBy,
        bool sortDescending,
        bool? isActive)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            string lowerSearchTerm = searchTerm.ToLower();

            query = query.Where(u =>
                u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                u.LastName.ToLower().Contains(lowerSearchTerm) ||
                u.Email.ToLower().Contains(lowerSearchTerm) ||
                u.PhoneNumber.ToLower().Contains(lowerSearchTerm));
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        int totalCount = await query.CountAsync();

        var paginatedUsers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (paginatedUsers, totalCount);
    }



    public void Update(User user)
    {
        _dbContext.Users.Update(user);
        _dbContext.SaveChanges();
    }
}
