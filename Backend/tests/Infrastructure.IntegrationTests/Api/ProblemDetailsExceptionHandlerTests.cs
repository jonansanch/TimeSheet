using System.Text.Json;
using KPG.Timesheet.Api.Infrastructure;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Api;

public class ProblemDetailsExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenDomainRuleException_ReturnsBadRequestProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handled = await handler.TryHandleAsync(
            httpContext,
            new DomainRuleException("Regla de prueba."),
            CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        responseBody.Position = 0;
        using var json = await JsonDocument.ParseAsync(responseBody);
        json.RootElement.GetProperty("title").GetString().Should().Be("Regla de negocio invalida.");
        json.RootElement.GetProperty("detail").GetString().Should().Be("Regla de prueba.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WhenNotFoundException_ReturnsNotFoundProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handled = await handler.TryHandleAsync(
            httpContext,
            new NotFoundException("empleado no existe"),
            CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        responseBody.Position = 0;
        using var json = await JsonDocument.ParseAsync(responseBody);
        json.RootElement.GetProperty("title").GetString().Should().Be("El recurso especificado no fue encontrado.");
        json.RootElement.GetProperty("detail").GetString().Should().Be("empleado no existe");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnauthorizedAccessException_ReturnsUnauthorizedProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handled = await handler.TryHandleAsync(
            httpContext,
            new UnauthorizedAccessException(),
            CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        responseBody.Position = 0;
        using var json = await JsonDocument.ParseAsync(responseBody);
        json.RootElement.GetProperty("title").GetString().Should().Be("No autorizado.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task TryHandleAsync_WhenForbiddenAccessException_ReturnsForbiddenProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handled = await handler.TryHandleAsync(
            httpContext,
            new ForbiddenAccessException(),
            CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        responseBody.Position = 0;
        using var json = await JsonDocument.ParseAsync(responseBody);
        json.RootElement.GetProperty("title").GetString().Should().Be("Acceso prohibido.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status403Forbidden);
    }
}
