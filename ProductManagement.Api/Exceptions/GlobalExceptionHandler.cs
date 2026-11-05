using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace ProductManagement.Api.Exceptions;
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(
            exception,
            "An unexpected error occurred.");

        if (exception is DuplicateSkuException duplicateSkuException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 409,
                message = duplicateSkuException.Message
            }, ct);

            return true;
        }

        if (exception is BusinessNotAssociatedException businessNotAssociatedException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 409,
                message = businessNotAssociatedException.Message
            }, ct);

            return true;
        }

        if (exception is OrderConflictException orderConflictException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 409,
                message = orderConflictException.Message
            }, ct);

            return true;
        }

        if (exception is OrderNotFoundException orderNotFoundException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status404NotFound;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 404,
                message = orderNotFoundException.Message
            }, ct);

            return true;
        }

        var postgresException = FindPostgresException(exception);

        if (postgresException?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            var message = postgresException.ConstraintName?.Contains(
                "IX_Suppliers_BusinessId_Email",
                StringComparison.OrdinalIgnoreCase)
                == true
                ? "A supplier with this email already exists in this business."
                : "A product with the specified SKU already exists.";

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 409,
                message
            }, ct);

            return true;
        }

        if (postgresException?.SqlState == PostgresErrorCodes.DeadlockDetected)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 409,
                message = "A concurrent database update conflicted with this request. Please retry."
            }, ct);

            return true;
        }

        if (exception is ProductNotFoundException productNotFoundException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status404NotFound;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 404,
                message = "This product was not found."
            }, ct);

            return true;
        }

        if (exception is ValidationException validationException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 400,
                message = validationException.Message
            }, ct);

            return true;
        }

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            status = 500,
            message = "An unexpected error occurred."
        }, ct);

        return true;
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (Exception? current = exception;
             current is not null;
             current = current.InnerException)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }
        }

        return null;
    }
}
