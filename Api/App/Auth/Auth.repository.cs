using Api.App.Auth.Interfaces;
using Api.App.User.Entities;
using Api.Infra;
using Microsoft.EntityFrameworkCore;

namespace Api.App.Auth;

public class AuthRepository(AppDbContext db) : IAuthRepository
{
    public async Task<UserEntity?> GetByIdentifyAsync(string identify)
    {
        return await db.Users.FirstOrDefaultAsync(x => x.Identify == identify);
    }

    public async Task<bool> ExistsByIdentifyAsync(string identify)
    {
        return await db.Users.AnyAsync(x => x.Identify == identify);
    }

    public async Task<int> GetTotalCountAsync(string? keyword)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.Identify.Contains(keyword.Trim()));

        return await query.CountAsync();
    }


    public async Task AddAsync(UserEntity user)
    {
        await db.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
}