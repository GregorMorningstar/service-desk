using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Api.Auth;

public interface ICurrentUser
{
    string Id { get; }
    bool IsAdmin { get; }
    bool IsTechnician { get; }
}

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal Principal =>
        accessor.HttpContext?.User ?? throw new InvalidOperationException("No HTTP context.");

    public string Id =>
        Principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? throw new InvalidOperationException("Authenticated user has no 'sub' claim.");

    public bool IsAdmin => Principal.IsInRole(Roles.Administrator);
    public bool IsTechnician => Principal.IsInRole(Roles.Moderator);
}
