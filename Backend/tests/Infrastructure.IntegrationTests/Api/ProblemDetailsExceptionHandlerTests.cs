using System.Text.Json;
using KPG.Timesheet.Api.Infrastructure;
using KPG.Timesheet.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Api;

public class ProblemDetailsExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenDomainRuleException_ReturnsBadRequestProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler();
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
}
