using System.ComponentModel.DataAnnotations;

namespace Api.Auth;

public class JwtOptions
{
    public const string Section = "Jwt";

    /// <summary>HMAC signing key. Comes from the JWT_KEY env var, never from appsettings.</summary>
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "ServiceDesk";

    [Required]
    public string Audience { get; set; } = "ServiceDesk";

    [Range(1, 120)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays { get; set; } = 7;
}
