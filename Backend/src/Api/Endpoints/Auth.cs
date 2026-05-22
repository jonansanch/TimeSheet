using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KPG.Timesheet.Application.Features.Auth.Commands.Login;
using KPG.Timesheet.Application.Features.Auth.Commands.Logout;
using KPG.Timesheet.Application.Features.Auth.Commands.Refresh;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Auth : IEndpointGroup
{
    public static string RoutePrefix => "/api/auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login").AllowAnonymous();
        groupBuilder.MapPost(Refresh, "refresh").AllowAnonymous();
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
        groupBuilder.MapGet(Me, "me").RequireAuthorization();
    }

    [EndpointSummary("Iniciar sesión")]
    [EndpointDescription("Valida credenciales y retorna un JWT de acceso y un refresh token.")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Problem(
                title: "Credenciales inválidas.",
                detail: "El usuario o la contraseña son incorrectos.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9457");
        }
    }

    [EndpointSummary("Renovar sesión")]
    [EndpointDescription("Recibe un refresh token válido y retorna un nuevo JWT y un nuevo refresh token (rotación).")]
    [ProducesResponseType<RefreshTokenResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Problem(
                title: "Sesión expirada.",
                detail: "El refresh token es inválido o ha expirado. Inicie sesión nuevamente.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9457");
        }
    }

    [EndpointSummary("Usuario actual")]
    [EndpointDescription("Retorna el userId, email y roles del usuario autenticado.")]
    [ProducesResponseType<MeResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static IResult Me(HttpContext context)
    {
        var user = context.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? string.Empty;
        var email = user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? string.Empty;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Results.Ok(new MeResponseDto(userId, email, roles));
    }

    [EndpointSummary("Cerrar sesión")]
    [EndpointDescription("Revoca el refresh token activo del usuario.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> Logout(
        [FromBody] LogoutCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }
}
