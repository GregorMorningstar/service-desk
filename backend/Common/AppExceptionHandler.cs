using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Api.Common;

public class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not AppException appException)
            return false;

        ProblemDetails details = appException is ValidationException validation
            ? new ValidationProblemDetails(validation.Errors)
            : new ProblemDetails();

        details.Status = appException.StatusCode;
        details.Title = ReasonPhrases.GetReasonPhrase(appException.StatusCode);
        details.Detail = appException.Message;

        context.Response.StatusCode = appException.StatusCode;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = details,
            Exception = exception,
        });
    }
}
