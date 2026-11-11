using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public record AccessToken(string Token, int ExpiresInSeconds);

public interface ITokenService
{
    AccessToken CreateAccessToken(AppUser user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    string Hash(string refreshToken);
}

public class TokenService(IOptions<JwtOptions> options, TimeProvider time) : ITokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public AccessToken CreateAccessToken(AppUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AuthSetup.SecurityStampClaim, user.SecurityStamp ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(AuthSetup.RoleClaim, r)));

        var lifetime = TimeSpan.FromMinutes(_jwt.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = time.GetUtcNow().Add(lifetime).UtcDateTime,
            SigningCredentials = new SigningCredentials(AuthSetup.SigningKey(_jwt), SecurityAlgorithms.HmacSha256),
        };

        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), (int)lifetime.TotalSeconds);
    }

    public string GenerateRefreshToken() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

    public string Hash(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
