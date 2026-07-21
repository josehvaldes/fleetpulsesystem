using FleetPulse.SignalRHub.Validators;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.SignalRHub.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            var (statusCode, title) = exception switch
            {
                ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            logger.LogError(
                exception,
                "Exception: {Type} | Status: {StatusCode} | Message: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);

            ProblemDetails problemDetails = exception switch
            {
                ValidationException validationException => new ValidationProblemDetails(validationException.Errors)
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path
                },
                _ => new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path
                }
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

            return true; // true = exception is handled, do not propagate further
        }
    }
}
