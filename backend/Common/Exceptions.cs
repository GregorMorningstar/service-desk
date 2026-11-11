using Microsoft.AspNetCore.Identity;

namespace Api.Common;

/// <summary>Errors thrown by services; AppExceptionHandler turns them into ProblemDetails.</summary>
public abstract class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message = "Nie znaleziono zasobu.")
    : AppException(StatusCodes.Status404NotFound, message);

public class ForbiddenException(string message = "Brak uprawnień do tej operacji.")
    : AppException(StatusCodes.Status403Forbidden, message);

public class ConflictException(string message)
    : AppException(StatusCodes.Status409Conflict, message);

public class UnauthorizedException(string message)
    : AppException(StatusCodes.Status401Unauthorized, message);

public class ValidationException(IDictionary<string, string[]> errors)
    : AppException(StatusCodes.Status400BadRequest, "Dane są niepoprawne.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public static ValidationException For(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });

    // Keyed by Identity error code (DuplicateUserName, PasswordTooShort, ...) so the frontend can translate them.
    public static ValidationException From(IdentityResult result) =>
        new(result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
}
