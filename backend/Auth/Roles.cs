namespace Api.Auth;

/// <summary>
/// Moderator is the "technician" from the spec: works on tickets assigned to them.
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Moderator = "Moderator";
    public const string User = "User";

    public static readonly string[] All = [Administrator, Moderator, User];
}

public static class Policies
{
    public const string Admin = "Admin";

    /// <summary>Administrator or Moderator.</summary>
    public const string Staff = "Staff";
}

public static class RateLimits
{
    public const string Auth = "auth";
}
