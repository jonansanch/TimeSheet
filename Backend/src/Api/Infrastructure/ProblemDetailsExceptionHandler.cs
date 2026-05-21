using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KPG.Timesheet.Domain.Exceptions;
using ForbiddenAccessException = KPG.Timesheet.Application.Common.Exceptions.ForbiddenAccessException;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Api.Infrastructure;

/// <summary>
/// Converts well-known application exceptions into RFC 9110-compliant <see cref="ProblemDetails"/> responses,
/// mapping <see cref="ValidationException"/> → 400, <see cref="NotFoundException"/> → 404,
/// <see cref="UnauthorizedAccessException"/> → 401, and <see cref="ForbiddenAccessException"/> → 403.
/// Unrecognised exceptions are not handled and fall through to the default middleware.
/// </summary>
public class ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, (ProblemDetails)new ValidationProblemDetails(ve.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            }),
            DomainRuleException dre => (StatusCodes.Status400BadRequest, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Regla de negocio invalida.",
                Detail = dre.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            }),
            NotFoundException ne => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "El recurso especificado no fue encontrado.",
                Detail = ne.Message
            }),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "No autorizado.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2"
            }),
            BadHttpRequestException bhe => (StatusCodes.Status400BadRequest, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Solicitud invalida.",
                Detail = bhe.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            }),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Acceso prohibido.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4"
            }),
            _ => (StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            })
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
