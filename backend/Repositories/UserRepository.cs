using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public record UserWithRoles(AppUser User, IList<string> Roles);

public interface IUserRepository
{
    Task<List<UserWithRoles>> GetAllWithRolesAsync(string? role = null);
    Task<List<AppUser>> GetByIdsAsync(IReadOnlyCollection<string> ids);

    /// <summary>Tracked user (with address) for edits that go around UserManager.</summary>
    Task<AppUser?> GetForUpdateAsync(string id);

    Task SaveChangesAsync();
}

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<List<UserWithRoles>> GetAllWithRolesAsync(string? role = null)
    {
        var users = await db.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();

        // One query for all role assignments instead of GetRolesAsync per user.
        var assignments = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            select new { ur.UserId, r.Name }
        ).ToListAsync();
        var rolesByUser = assignments.ToLookup(a => a.UserId, a => a.Name!);

        return users
            .Select(u => new UserWithRoles(u, rolesByUser[u.Id].ToList()))
            .Where(u => role is null || u.Roles.Contains(role))
            .ToList();
    }

    public Task<List<AppUser>> GetByIdsAsync(IReadOnlyCollection<string> ids) =>
        db.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).ToListAsync();

    public Task<AppUser?> GetForUpdateAsync(string id) => db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
