using Host.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Shared.Domain.Exceptions;
using Xunit;

namespace Host.Tests.MiddlewareTests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Concurrency_conflict_returns_409()
    {
        var status = await StatusFor(new DbUpdateConcurrencyException("row changed"));

        Assert.Equal(StatusCodes.Status409Conflict, status);
    }

    [Fact]
    public async Task Unique_violation_returns_409()
    {
        var unique = new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation);

        var status = await StatusFor(new DbUpdateException("save failed", unique));

        Assert.Equal(StatusCodes.Status409Conflict, status);
    }

    [Fact]
    public async Task Other_database_error_stays_500()
    {
        var notNull = new PostgresException("null value", "ERROR", "ERROR", PostgresErrorCodes.NotNullViolation);

        var status = await StatusFor(new DbUpdateException("save failed", notNull));

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
    }

    [Fact]
    public async Task Domain_exception_still_returns_400()
    {
        var status = await StatusFor(new DomainException("invalid"));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
    }

    private static async Task<int> StatusFor(Exception thrown)
    {
        var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(_ => throw thrown,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        return context.Response.StatusCode;
    }
}
