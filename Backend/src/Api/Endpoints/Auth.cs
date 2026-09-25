using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KPG.Timesheet.Application.Features.Auth.Commands.ChangePassword;
using KPG.Timesheet.Application.Features.Auth.Commands.ForgotPassword;
using KPG.Timesheet.Application.Features.Auth.Commands.Login;
using KPG.Timesheet.Application.Features.Auth.Commands.Logout;
using KPG.Timesheet.Application.Features.Auth.Commands.Refresh;
using KPG.Timesheet.Application.Features.Auth.Commands.ResetPassword;
using KPG.Timesheet.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Auth : IEndpointGroup
{
    public static string RoutePrefix => "/api/auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login").AllowAnonymous().RequireRateLimiting("login");
        groupBuilder.MapPost(Refresh, "refresh").AllowAnonymous();
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
        groupBuilder.MapGet(Me, "me").RequireAuthorization();
        groupBuilder.MapPost(ChangePassword, "change-password").RequireAuthorization();
        groupBuilder.MapPost(ForgotPassword, "forgot-password").AllowAnonymous();
        groupBuilder.MapPost(ResetPassword, "reset-password").AllowAnonymous();
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
    [EndpointDescription("Retorna los datos actuales del usuario autenticado desde la base de datos.")]
    [ProducesResponseType<MeResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Me(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var principal = context.User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var nacionalidadDemostrativa = claims.Any(claim =>
            claim.Type == ApplicationUserClaimTypes.NacionalidadDemostrativa
            && bool.TryParse(claim.Value, out var value)
            && value);

        return Results.Ok(new MeResponseDto(
            user.Id,
            user.Email ?? string.Empty,
            roles.ToList(),
            user.NombreCompleto,
            user.CodigoPais,
            nacionalidadDemostrativa));
    }

    [EndpointSummary("Cambiar contraseña")]
    [EndpointDescription("Cambia la contraseña del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Succeeded
            ? Results.NoContent()
            : Results.Problem(
                title: "No se pudo cambiar la contraseña.",
                detail: string.Join(" ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc9457");
    }

    [EndpointSummary("Solicitar restablecimiento de contraseña")]
    [EndpointDescription("Envía un correo con enlace de reset. Siempre retorna 204 para no revelar si el email existe.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Restablecer contraseña con token")]
    [EndpointDescription("Valida el token de reset y establece la nueva contraseña.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var ok = await sender.Send(command, cancellationToken);
        return ok
            ? Results.NoContent()
            : Results.Problem(
                title: "No se pudo restablecer la contraseña.",
                detail: "El enlace expiró o no es válido. Solicita uno nuevo.",
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc9457");
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
