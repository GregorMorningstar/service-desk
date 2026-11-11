using Api.Auth;
using Api.Common;
using Api.Data;
using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto> LoginAsync(LoginDto dto);
    Task<TokenResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<UserDto> GetCurrentUserAsync();
}

public class AuthService(
    UserManager<AppUser> userManager,
    ITokenService tokens,
    IRefreshTokenRepository refreshTokens,
    ICurrentUser currentUser,
    IOptions<JwtOptions> jwt,
    TimeProvider time) : IAuthService
{
    private const string InvalidCredentials = "Nieprawidłowy email lub hasło.";
    private const string LockedOut = "Konto jest zablokowane. Spróbuj ponownie później lub skontaktuj się z administratorem.";
    private const string InvalidRefreshToken = "Sesja wygasła. Zaloguj się ponownie.";

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw ValidationException.From(result);

        result = await userManager.AddToRoleAsync(user, Roles.User);
        if (!result.Succeeded)
            throw ValidationException.From(result);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedException(InvalidCredentials);

        if (await userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException(LockedOut);

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            // 5 failed attempts lock the account for 15 minutes (see AuthSetup).
            await userManager.AccessFailedAsync(user);
            throw new UnauthorizedException(await userManager.IsLockedOutAsync(user) ? LockedOut : InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user);
    }

    public async Task<TokenResponseDto> RefreshAsync(string refreshToken)
    {
        var stored = await refreshTokens.GetByHashAsync(tokens.Hash(refreshToken))
            ?? throw new UnauthorizedException(InvalidRefreshToken);

        if (stored.RevokedAt is not null)
        {
            // An already rotated token came back: treat it as stolen and end all sessions of this user.
            await refreshTokens.RevokeAllForUserAsync(stored.UserId, Now);
            throw new UnauthorizedException(InvalidRefreshToken);
        }

        if (stored.ExpiresAt <= Now)
            throw new UnauthorizedException(InvalidRefreshToken);

        var user = await userManager.FindByIdAsync(stored.UserId)
            ?? throw new UnauthorizedException(InvalidRefreshToken);

        return await IssueTokensAsync(user, replacing: stored);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await refreshTokens.GetByHashAsync(tokens.Hash(refreshToken));
        if (stored is null || stored.RevokedAt is not null)
            return;

        stored.RevokedAt = Now;
        await refreshTokens.SaveChangesAsync();
    }

    public async Task<UserDto> GetCurrentUserAsync()
    {
        var user = await userManager.FindByIdAsync(currentUser.Id) ?? throw new NotFoundException();
        var roles = await userManager.GetRolesAsync(user);
        return new UserDto(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, roles);
    }

    private async Task<TokenResponseDto> IssueTokensAsync(AppUser user, RefreshToken? replacing = null)
    {
        var roles = await userManager.GetRolesAsync(user);
        var access = tokens.CreateAccessToken(user, roles);
        var refresh = tokens.GenerateRefreshToken();

        var entity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.Hash(refresh),
            CreatedAt = Now,
            ExpiresAt = Now.AddDays(jwt.Value.RefreshTokenDays),
        };

        if (replacing is not null)
        {
            replacing.RevokedAt = Now;
            replacing.ReplacedByHash = entity.TokenHash;
        }

        refreshTokens.Add(entity);
        await refreshTokens.SaveChangesAsync();

        return new TokenResponseDto("Bearer", access.Token, access.ExpiresInSeconds, refresh);
    }
}
