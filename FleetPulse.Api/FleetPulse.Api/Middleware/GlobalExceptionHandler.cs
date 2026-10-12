using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlertsValidationException = FleetPulse.Alerts.Application.Common.Exceptions.ValidationException;
using DriversValidationException = FleetPulse.Drivers.Application.Common.Exceptions.ValidationException;

namespace FleetPulse.Api.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var validationErrors = exception switch
            {
                DriversValidationException e => e.Errors,
                AlertsValidationException e => e.Errors,
                _ => null
            };

            var (statusCode, title) = exception switch
            {
                DriversValidationException or AlertsValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            logger.LogError(
                exception,
                "Exception: {Type} | Status: {StatusCode} | Message: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);

            ProblemDetails problemDetails = validationErrors is not null
                ? new ValidationProblemDetails(validationErrors)
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path
                }
                : new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path
                };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

            return true;
        }
    }
}
