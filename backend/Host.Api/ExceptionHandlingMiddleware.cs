using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Domain.Exceptions;

namespace Host.Api;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException exception)
        {
            await Results.Problem(statusCode: StatusCodes.Status404NotFound, title: exception.Message)
                .ExecuteAsync(context);
        }
        catch (DomainException exception)
        {
            await Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: exception.Message)
                .ExecuteAsync(context);
        }
        catch (DbUpdateException exception) when (IsWriteConflict(exception))
        {
            // Two requests changed the same row (optimistic concurrency) or inserted the same unique key.
            _logger.LogWarning(exception, "Write conflict for {Method} {Path}", context.Request.Method, context.Request.Path);
            await Results.Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "The data was changed by another request. Reload and try again.")
                .ExecuteAsync(context);
        }
        catch (BadHttpRequestException exception)
        {
            // Raised by Kestrel, e.g. 413 when a request body is larger than the limit in Program.cs.
            await Results.Problem(statusCode: exception.StatusCode, title: exception.Message)
                .ExecuteAsync(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.")
                .ExecuteAsync(context);
        }
    }

    private static bool IsWriteConflict(DbUpdateException exception) =>
        exception is DbUpdateConcurrencyException
        || exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
