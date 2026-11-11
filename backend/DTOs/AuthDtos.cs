using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public record RegisterDto
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; init; } = string.Empty;

    /// <summary>Min. 8 chars, upper and lower case letter, digit and a special character.</summary>
    [Required, StringLength(100)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; init; } = string.Empty;
}

public record LoginDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

/// <param name="ExpiresIn">Access token lifetime in seconds.</param>
public record TokenResponseDto(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);

public record UserDto(string Id, string Email, string FirstName, string LastName, IList<string> Roles);
