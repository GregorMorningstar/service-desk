using System.ComponentModel.DataAnnotations;
using Api.Data;

namespace Api.DTOs;

/// <summary>Short user reference embedded in tickets, comments, assets.</summary>
public record UserRefDto(string Id, string Email, string FullName)
{
    public static UserRefDto From(AppUser user) =>
        new(user.Id, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}".Trim());
}

public record UserAdminDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IList<string> Roles,
    bool IsLocked);

public record UserDetailsDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IList<string> Roles,
    bool IsLocked,
    AddressDto? Address);

public record ChangeRoleDto
{
    /// <summary>Administrator, Moderator or User.</summary>
    [Required]
    public string Role { get; init; } = string.Empty;
}

public record SetLockDto
{
    public bool Locked { get; init; }
}
