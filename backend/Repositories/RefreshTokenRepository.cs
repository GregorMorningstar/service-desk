using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    void Add(RefreshToken token);
    Task RevokeAllForUserAsync(string userId, DateTime now);
    Task SaveChangesAsync();
}

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);

    public async Task RevokeAllForUserAsync(string userId, DateTime now)
    {
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();
        foreach (var token in active)
            token.RevokedAt = now;
        await db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
