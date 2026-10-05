using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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

        if (exception is DbUpdateException dbException &&
            dbException.InnerException is SqlException sqlException &&
            (sqlException.Number == 2601 || sqlException.Number == 2627))
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status = 409,
                message = "A product with the specified SKU already exists."
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
}