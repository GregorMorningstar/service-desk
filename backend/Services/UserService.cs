using Api.Auth;
using Api.Common;
using Api.Data;
using Api.DTOs;
using Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserAdminDto>> GetAllAsync(string? role);
    Task<IReadOnlyList<UserRefDto>> GetTechniciansAsync();
    Task<UserAdminDto> ChangeRoleAsync(string id, string role);
    Task<UserAdminDto> SetLockAsync(string id, bool locked);
    Task<UserDetailsDto> GetDetailsAsync(string id);
    Task<UserDetailsDto> SetAddressAsync(string id, AddressDto? address);
}

public class UserService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    UserManager<AppUser> userManager,
    ICurrentUser currentUser,
    TimeProvider time) : IUserService
{
    // DateTimeOffset.MaxValue overflows MySQL's datetime column.
    private static readonly DateTimeOffset LockedForever = new(2999, 12, 31, 0, 0, 0, TimeSpan.Zero);

    public async Task<IReadOnlyList<UserAdminDto>> GetAllAsync(string? role)
    {
        var list = await users.GetAllWithRolesAsync(role);
        return list.Select(u => ToDto(u.User, u.Roles)).ToList();
    }

    public async Task<IReadOnlyList<UserRefDto>> GetTechniciansAsync()
    {
        var technicians = await userManager.GetUsersInRoleAsync(Roles.Moderator);
        var admins = await userManager.GetUsersInRoleAsync(Roles.Administrator);

        return technicians.Concat(admins)
            .DistinctBy(u => u.Id)
            .OrderBy(u => u.Email)
            .Select(UserRefDto.From)
            .ToList();
    }

    public async Task<UserAdminDto> ChangeRoleAsync(string id, string role)
    {
        if (role is not (Roles.User or Roles.Moderator))
            throw ValidationException.For("role", "Rolę można ustawić tylko na Użytkownik lub Moderator.");

        var user = await FindAsync(id);

        // One role per user.
        var current = await userManager.GetRolesAsync(user);
        if (current.Contains(Roles.Administrator))
            throw new ConflictException("Roli administratora nie można zmienić.");

        Check(await userManager.RemoveFromRolesAsync(user, current));
        Check(await userManager.AddToRoleAsync(user, role));

        // Old access tokens carry old roles. New stamp = they stop working and the client refreshes.
        Check(await userManager.UpdateSecurityStampAsync(user));

        return ToDto(user, [role]);
    }

    public async Task<UserAdminDto> SetLockAsync(string id, bool locked)
    {
        var user = await FindAsync(id);
        if (user.Id == currentUser.Id)
            throw new ConflictException("Nie możesz zablokować własnego konta.");

        Check(await userManager.SetLockoutEnabledAsync(user, true));
        Check(await userManager.SetLockoutEndDateAsync(user, locked ? LockedForever : null));
        Check(await userManager.ResetAccessFailedCountAsync(user));

        if (locked)
        {
            Check(await userManager.UpdateSecurityStampAsync(user));
            await refreshTokens.RevokeAllForUserAsync(user.Id, time.GetUtcNow().UtcDateTime);
        }

        return ToDto(user, await userManager.GetRolesAsync(user));
    }

    public async Task<UserDetailsDto> GetDetailsAsync(string id)
    {
        var user = await FindAsync(id);
        return await ToDetailsAsync(user);
    }

    public async Task<UserDetailsDto> SetAddressAsync(string id, AddressDto? address)
    {
        // Saved through the DbContext: UserManager.UpdateAsync marks the owned row as Modified,
        // which breaks when the address is being added for the first time.
        var user = await users.GetForUpdateAsync(id) ?? throw new NotFoundException("Nie znaleziono użytkownika.");
        user.Address = address?.ApplyTo(user.Address);
        await users.SaveChangesAsync();

        return await ToDetailsAsync(user);
    }

    private async Task<UserDetailsDto> ToDetailsAsync(AppUser u) => new(
        u.Id,
        u.Email ?? string.Empty,
        u.FirstName,
        u.LastName,
        await userManager.GetRolesAsync(u),
        IsLocked: u.LockoutEnd is { } end && end > time.GetUtcNow(),
        AddressDto.From(u.Address));

    private async Task<AppUser> FindAsync(string id) =>
        await userManager.FindByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono użytkownika.");

    private UserAdminDto ToDto(AppUser u, IList<string> roles) => new(
        u.Id,
        u.Email ?? string.Empty,
        u.FirstName,
        u.LastName,
        roles,
        IsLocked: u.LockoutEnd is { } end && end > time.GetUtcNow());

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw ValidationException.From(result);
    }
}
